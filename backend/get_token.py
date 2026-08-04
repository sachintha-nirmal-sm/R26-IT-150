"""
get_token.py — Utility to fetch a fresh Firebase ID Token for Swagger UI testing.

Usage:
    python get_token.py <YOUR_WEB_API_KEY> [email] [password]
"""

import sys
import os
import requests

def get_id_token(api_key: str, email: str = "student@example.com", password: str = "student123"):
    url = f"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={api_key}"
    payload = {
        "email": email,
        "password": password,
        "returnSecureToken": True
    }
    
    response = requests.post(url, json=payload)
    data = response.json()
    
    if response.status_code != 200:
        error_msg = data.get("error", {}).get("message", "Unknown error")
        print(f"\nLogin failed ({response.status_code}): {error_msg}")
        if error_msg == "INVALID_KEY":
            print("Check that your Web API Key is correct.")
        elif error_msg == "EMAIL_NOT_FOUND" or error_msg == "INVALID_PASSWORD":
            print(f"Check that '{email}' exists in Firebase Auth. Did you run the seed_users.py script?")
        sys.exit(1)
        
    id_token = data.get("idToken")
    bearer_token = f"Bearer {id_token}"

    # Also write to token.txt for easy access
    token_file = os.path.join(os.path.dirname(__file__), "token.txt")
    with open(token_file, "w", encoding="utf-8") as f:
        f.write(bearer_token)
    
    print("\n" + "="*60)
    print("SUCCESS! Copy the line below into Swagger UI authorization field:")
    print("="*60 + "\n")
    print(bearer_token)
    print("\n" + "="*60)
    print(f"(Token also saved to: {token_file})")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        env_key = os.getenv("FIREBASE_WEB_API_KEY")
        if env_key:
            get_id_token(env_key)
        else:
            print("Usage: python get_token.py <YOUR_WEB_API_KEY> [email] [password]")
            sys.exit(1)
    else:
        key = sys.argv[1]
        user_email = sys.argv[2] if len(sys.argv) > 2 else "student@example.com"
        user_pass = sys.argv[3] if len(sys.argv) > 3 else "student123"
        get_id_token(key, user_email, user_pass)
