from fastapi import FastAPI, APIRouter, HTTPException
from pydantic import BaseModel
from ml_models.plant_disease import rag 
# import ml_models.plant_disease.rag as rag
router = APIRouter()

class QueryRequest(BaseModel):
    query: str

# pdf_path = r"C:\Users\dttan\Downloads\computer-vision\CV-Bài 02 - Chương 1 - OpenCV-PyNum.pdf"
# rag_chain = get_qa_chain(pdf_path)

@router.get("/")
def hello():
    return {"message": "Hello from Chatbot API"}

@router.post("/chat")
def chatbot(query: QueryRequest):
    try:
        print("Chatbot endpoint accessed")
        response = rag.rag_chain.invoke({"input": query.query})
        return {"response": response["answer"]}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
