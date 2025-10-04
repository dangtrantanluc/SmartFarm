# model_utils.py
import torch
import torch.nn.functional as F
from torchvision import transforms, models
from PIL import Image
import io
import json

IMG_SIZE = 224
IMAGENET_MEAN = [0.485, 0.456, 0.406]
IMAGENET_STD  = [0.229, 0.224, 0.225]

transform = transforms.Compose([
    transforms.Lambda(lambda img: img.convert("RGB")),
    transforms.Resize((IMG_SIZE, IMG_SIZE)),
    transforms.ToTensor(),
    transforms.Normalize(IMAGENET_MEAN, IMAGENET_STD)
])

def load_model(model_path: str, device=None, num_classes=None, class_names_path=None):
    device = device or (torch.device("cuda") if torch.cuda.is_available() else torch.device("cpu"))
    # Tùy: nếu bạn lưu state_dict
    model = models.resnet50(weights=None)
    if num_classes is None:
        # fallback, user nên truyền num_classes
        pass
    else:
        model.fc = torch.nn.Linear(model.fc.in_features, num_classes)
    model.load_state_dict(torch.load(model_path, map_location=device))
    model.to(device)
    model.eval()
    # load class names (nếu có)
    class_names = None
    if class_names_path:
        with open(class_names_path, 'r', encoding='utf-8') as f:
            class_names = [x.strip() for x in f.readlines()]
    return model, class_names, device

def predict_image_bytes(model, device, image_bytes, class_names=None, top_k=3):
    img = Image.open(io.BytesIO(image_bytes)).convert("RGB")
    x = transform(img).unsqueeze(0).to(device)  # shape [1,3,H,W]
    with torch.no_grad():
        logits = model(x)
        probs = F.softmax(logits, dim=1)
        topk = torch.topk(probs, k=top_k)
        values = topk.values.cpu().numpy()[0].tolist()
        indices = topk.indices.cpu().numpy()[0].tolist()
    results = []
    for idx, score in zip(indices, values):
        label = class_names[idx] if class_names else str(idx)
        results.append({"label": label, "score": float(score)})
    return results
