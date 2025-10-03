"""
Custom LLM để gọi local API server
"""

import requests
from langchain.llms.base import LLM
from typing import Optional, List, Any

class LocalAPILLM(LLM):
    api_url: str = "http://localhost:5000/generate"
    max_length: int = 256
    temperature: float = 0.7
    
    @property
    def _llm_type(self) -> str:
        return "local_api"
    
    def _call(
        self,
        prompt: str,
        stop: Optional[List[str]] = None,
        run_manager: Optional[Any] = None,
        **kwargs: Any,
    ) -> str:
        try:
            response = requests.post(
                self.api_url,
                json={
                    'prompt': prompt,
                    'max_length': self.max_length,
                    'temperature': self.temperature
                },
                timeout=30
            )
            
            if response.status_code == 200:
                result = response.json()
                return result.get('generated_text', '')
            else:
                return f"API Error: {response.status_code}"
                
        except Exception as e:
            return f"Connection Error: {str(e)}"