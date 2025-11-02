from langchain_text_splitters import CharacterTextSplitter
from langchain_community.vectorstores import FAISS
from langchain_huggingface import HuggingFaceEmbeddings, HuggingFaceEndpoint
from langchain_core.prompts import ChatPromptTemplate
from langchain.chains.combine_documents import create_stuff_documents_chain
from langchain.chains import create_retrieval_chain
from transformers import BitsAndBytesConfig
from langchain.docstore.document import Document
from huggingface_hub import login
from langchain_huggingface import HuggingFacePipeline
from transformers import pipeline, AutoModelForCausalLM, AutoTokenizer, AutoModelForSeq2SeqLM
import pandas as pd
import torch
import json

# Đăng nhập HuggingFace Hub (điền token của bạn vào)
# login(token="")

device = "cuda" if torch.cuda.is_available() else "cpu"

data = pd.read_json(r'D:\DangTranTanLuc\Chatbot\ml_models\data\data.json')

# Prompt hệ thống
system_prompt = "Context: {context}\n\nQuestion: {input}"


quant_config = BitsAndBytesConfig(load_in_4bit=True,
                                    bnb_4bit_quant_type="nf4",
                                    bnb_4bit_use_double_quant=True,
                                    bnb_4bit_compute_type=torch.float16)
tokenizer = AutoTokenizer.from_pretrained("Qwen/Qwen1.5-0.5B-Chat", use_fast=True)
model = AutoModelForCausalLM.from_pretrained("Qwen/Qwen1.5-0.5B-Chat", quantization_config=quant_config, device_map = device)

rag_prompt = ChatPromptTemplate.from_messages([
   
    ("human", "Context: {context}\n\nQuestion: {input}")
])


  
# Comment lại HuggingFace Pipeline để tránh lỗi
#Tạo pipeline local với cấu hình an toàn
print("Đang tải model ...")
pipe = pipeline(
    "text2text-generation",
    model=model,
    tokenizer=tokenizer,
    max_new_tokens=128,
    temperature=0.3,
    top_p=0.9,
    repetition_penalty=1.1
)

llm = HuggingFacePipeline(pipeline=pipe)

def get_qa_chain():
    # Load tài liệu PDF
    with open(r'D:\DangTranTanLuc\Chatbot\ml_models\data\data.json', 'r', encoding='utf-8') as f:
        data = json.load(f)
    documents = [Document(page_content=f"Hỏi: {d['question']}\nĐáp: {d['answer']}") for d in data]

    # Cắt nhỏ văn bản với chunk nhỏ hơn để tránh vượt quá giới hạn token
    text_splitter = CharacterTextSplitter(
        chunk_size=300, 
        chunk_overlap=50  
    )
    texts = text_splitter.split_documents(documents)

    embeddings = HuggingFaceEmbeddings(model_name="sentence-transformers/all-MiniLM-L6-v2")
    try:
        vectorstore = FAISS.load_local(r"D:\DangTranTanLuc\Chatbot\ml_models\plant_disease\faiss_plant_disease", embeddings, allow_dangerous_deserialization=True)
        print("FAISS vector store loaded successfully.")
    except Exception:
        vectorstore = FAISS.from_documents(texts, embeddings)
        vectorstore.save_local("faiss_plant_disease")
        print("Saved vector store locally after creation.")
    vectorstore = FAISS.from_documents(texts, embeddings)

    retriever = vectorstore.as_retriever(
        search_type="similarity", 
        search_kwargs={"k": 1}  
    )

    # Document chain (LLM + Prompt)
    document_chain = create_stuff_documents_chain(
        llm=llm,
        prompt=rag_prompt
    )

    # Retrieval chain
    rag_chain = create_retrieval_chain(
        retriever=retriever,
        combine_docs_chain=document_chain
    )
    print("RAG chain created successfully.")
    return rag_chain


# pdf_path = r"C:/Users/Admin/Downloads/S.pdf"


rag_chain = get_qa_chain()