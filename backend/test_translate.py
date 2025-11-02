from googletrans import Translator

translator = Translator()
result = translator.translate("Hello, how are you?", src='en', dest='vi')
print(result.text)  # -> "Xin chào, bạn khỏe không?"
