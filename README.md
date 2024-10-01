# 🐈 ☁️ Cat Discriminator Save Function

This is an Azure Function that facilitates uploading training photos of my cats to Azure Blob Storage. The function expects an HTTP 'POST' request with an image in base64-encoded plain text image as the request body. 

The function will generate a .jpg image file from the base64 text and upload it to Blob Storage for consumption later.