from datasets import load_dataset

ds = load_dataset("taidng/UIT-ViQuAD2.0")

ds["train"].to_csv("data/chatbot/train.csv", index=False)
ds["validation"].to_csv("data/chatbot/validation.csv", index=False)
ds["test"].to_csv("data/chatbot/test.csv", index=False)