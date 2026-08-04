"""
create_admin.py — Utility to securely create a new Admin account.

Usage:
    python create_admin.py <email> <password> <full_name>

Example:
    python create_admin.py teacher@example.com mysecurepassword "Mr. Teacher"
"""

import sys
from google.cloud import firestore
import firebase_admin.auth as firebase_auth

# Import our backend firebase setup
from app.core.firebase import auth, db

def create_admin(email: str, password: str, full_name: str):
    print(f"🚀 Creating new Admin account: {email}")

    # 1. Create Auth User
    try:
        new_user = auth.create_user(
            email=email,
            password=password,
            display_name=full_name
        )
        uid = new_user.uid
        print(f"✅ Firebase Auth user created with UID: {uid}")
    except firebase_auth.EmailAlreadyExistsError:
        print(f"❌ Error: An account with email '{email}' already exists.")
        sys.exit(1)
    except Exception as e:
        print(f"❌ Failed to create Auth user: {e}")
        sys.exit(1)

    # 2. Set Admin Custom Claim
    try:
        auth.set_custom_user_claims(uid, {"role": "admin"})
        print(f"✅ 'admin' custom claim set on Auth token.")
    except Exception as e:
        print(f"❌ Failed to set admin claim: {e}")
        auth.delete_user(uid) # cleanup
        sys.exit(1)

    # 3. Create users/{uid} Firestore Document
    try:
        now = firestore.SERVER_TIMESTAMP
        admin_doc = {
            "role": "admin",
            "status": "active",
            "fullName": full_name,
            "email": email,
            "createdAt": now,
            "updatedAt": now,
        }
        db.collection("users").document(uid).set(admin_doc)
        print(f"✅ Admin profile saved to Firestore 'users/{uid}'.")
    except Exception as e:
        print(f"❌ Failed to create Firestore profile: {e}")
        auth.delete_user(uid) # cleanup
        sys.exit(1)

    print("\n🎉 Admin successfully created!")
    print("You can now generate a token for this admin using get_token.py:")
    print(f"python get_token.py YOUR_REAL_WEB_API_KEY {email} {password}")

if __name__ == "__main__":
    if len(sys.argv) < 4:
        print("Usage: python create_admin.py <email> <password> <full_name>")
        print("Example: python create_admin.py teacher@school.com pass1234 \"Jane Doe\"")
        sys.exit(1)
        
    email = sys.argv[1]
    password = sys.argv[2]
    full_name = sys.argv[3]
    
    create_admin(email, password, full_name)
