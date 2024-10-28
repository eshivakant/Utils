pip install pandas requests openpyxl


import pandas as pd
import requests

# Read the Excel file
file_path = "input_file.xlsx"
df = pd.read_excel(file_path, engine='openpyxl')

# Define the endpoint
url = "https://example.com/api/process"  # Replace with your endpoint

# Process each row in column A, send data to the endpoint, and store the result
responses = []
for text in df['A']:
    response = requests.post(url, data={'text': text})
    if response.status_code == 200:
        responses.append(response.text)
    else:
        responses.append(None)  # Handle error responses gracefully

# Write the responses to column B
df['B'] = responses

# Save the modified DataFrame back to Excel
output_file = "output_file.xlsx"
df.to_excel(output_file, index=False, engine='openpyxl')
print(f"Processed file saved as {output_file}")
