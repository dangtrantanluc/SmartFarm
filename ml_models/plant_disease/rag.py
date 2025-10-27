from langchain_community.document_loaders import PyPDFLoader
from langchain_text_splitters import CharacterTextSplitter

# from langchain_text_splitters import RecursiveCharacterTextSplitter

from langchain_community.vectorstores import FAISS
from langchain_huggingface import HuggingFaceEmbeddings, HuggingFaceEndpoint
from langchain_core.prompts import ChatPromptTemplate
from langchain.chains.combine_documents import create_stuff_documents_chain
from langchain.chains import create_retrieval_chain
from langchain_community.llms.fake import FakeListLLM
from langchain_community.llms import Ollama
from huggingface_hub import login
from langchain_huggingface import HuggingFacePipeline
from transformers import pipeline, AutoModelForCausalLM, AutoTokenizer, AutoModelForSeq2SeqLM
import torch

# Đăng nhập HuggingFace Hub (điền token của bạn vào)
# login(token="")

# Prompt hệ thống
system_prompt = """Bạn là một trợ lý AI hỗ trợ tìm thông tin từ tài liệu.
Bạn sẽ được cung cấp các đoạn trích từ một tài liệu dài và một câu hỏi.
Hãy trả lời dựa trên ngữ cảnh đã cho.
Nếu không biết thì hãy nói "Tôi không biết", đừng bịa ra câu trả lời.
Luôn trả lời bằng tiếng Việt.
"""

rag_prompt = ChatPromptTemplate.from_messages([
    ("system", system_prompt),
    ("human", "Context: {context}\n\nQuestion: {input}")
])


tokenizer = AutoTokenizer.from_pretrained("google/flan-t5-small")
model = AutoModelForSeq2SeqLM.from_pretrained("google/flan-t5-small")

# Comment lại HuggingFace Pipeline để tránh lỗi
#Tạo pipeline local với cấu hình an toàn
print("Đang tải model ...")
# pipe = pipeline(
#     "text-generation",
#     model=model,
#     tokenizer=tokenizer,
#     device="cuda",
#     temperature=0.7,
#     max_new_tokens=100,
#     do_sample=True,
#     truncation=True
# ) 
pipe = pipeline(
    "text2text-generation",
    model=model,
    tokenizer=tokenizer,
    device=0 if torch.cuda.is_available() else -1,
    max_new_tokens=128
)
llm = HuggingFacePipeline(pipeline=pipe)

def get_qa_chain(pdf_path: str):
    # Load tài liệu PDF
    loader = PyPDFLoader(pdf_path)
    documents = loader.load()

    # Cắt nhỏ văn bản với chunk nhỏ hơn để tránh vượt quá giới hạn token
    text_splitter = CharacterTextSplitter(
        chunk_size=300, 
        chunk_overlap=50  
    )
    texts = text_splitter.split_documents(documents)

    # Embeddings + FAISS
    embeddings = HuggingFaceEmbeddings(model_name="sentence-transformers/all-mpnet-base-v2")
    vectorstore = FAISS.from_documents(texts, embeddings)
    retriever = vectorstore.as_retriever(
        search_type="similarity", 
        search_kwargs={"k": 2}  
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


pdf_path = r"C:/Users/Admin/Downloads/S.pdf"
rag_chain = get_qa_chain(pdf_path)