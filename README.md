# SmartFarm
# You must clone our code and cd into the app folder to run backend

# create virtual environment
python -m venv .venv

source venv/bin/activate   # macOS/Linux

venv\Scripts\activate      # Windows

# install requirement
pip install -r requirements.txt

# Run backend
fastapi dev main.py --host 0.0.0.0 --port 8000
