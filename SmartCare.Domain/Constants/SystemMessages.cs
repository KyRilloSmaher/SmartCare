using System;

namespace SmartCare.Domain.Constants
{
    public static class SystemMessages
    {
        // =====================
        // ✅ General Success
        // =====================
        public const string SUCCESS = "Operation completed successfully.";
        public const string OPERATION_SUCCESSFUL = "The operation was successful.";
        public const string DATA_RETRIEVED = "Data retrieved successfully.";
        public const string RECORD_ADDED = "Record added successfully.";
        public const string RECORD_UPDATED = "Record updated successfully.";
        public const string RECORD_DELETED = "Record deleted successfully.";
        public const string SETTINGS_SAVED = "Settings saved successfully.";
        // =====================
        // ⚠️ Common Validation / Errors
        // =====================
        public const string ERROR = "An unexpected error occurred. Please try again.";
        public const string FAILED = "The operation failed. Please try again.";
        public const string INVALID_INPUT = "Invalid input. Please check the provided data.";
        public const string RECORD_NOT_FOUND = "The requested record was not found.";
        public const string DUPLICATE_RECORD = "A record with similar data already exists.";
        public const string REQUIRED_FIELDS_MISSING = "Some required fields are missing.";
        public const string INVALID_TOKEN = "Invalid or expired token.";
        public const string DATABASE_ERROR = "A database error occurred.";
        public const string NETWORK_ERROR = "A network error occurred. Please try again later.";
        public const string SERVER_ERROR = "Internal server error.";
        public const string BAD_REQUEST = "Bad request. Please verify the input data.";
        public const string NULL_PARAMETER = "Null Parameter to the Function";
        // =====================
        // 👤 User & Auth
        // =====================
        public const string USER_CREATED = "User account created successfully.";
        public const string USER_UPDATED = "User account updated successfully.";
        public const string USER_DELETED = "User account deleted successfully.";
        public const string USER_VERIFIED = "User verified successfully.";
        public const string USER_NOT_FOUND = "User not found.";
        public const string UNAUTHORIZED = "Invalid or expired refresh token";
        public const string EMAIL_ALREADY_EXISTS = "The email address is already registered.";
        public const string USERNAME_ALREADY_EXISTS = "The username is already taken.";
        public const string PASSWORD_CHANGED = "Password changed successfully.";
        public const string PASSWORD_RESET_SUCCESS = "Password reset successfully.";
        public const string INVALID_CREDENTIALS = "Invalid username or password.";
        public const string ACCOUNT_LOCKED = "Your account is locked. Please contact support.";
        public const string EMAIL_NOT_CONFIRMED = "Please confirm your email before continuing.";
        public const string TOKEN_EXPIRED = "Your session has expired. Please log in again.";
        public const string TOKEN_GENERATED = "New Refresh Token has been generated";
        public const string LOGIN_SUCCESS = "Login successful.";
        public const string LOGOUT_SUCCESS = "Logout successful.";
        public const string ACCESS_DENIED = "Access denied. You do not have permission to perform this action.";
        public const string GENERATING_CODE_FAILED = "Failed To Generate Reset Code";
        public const string RESET_PASSWORD_CODE_SENT = "Reset Code Sent To Your Email Successfully";
        public const string INVALID_RESET_CODE = "The reset code is invalid or has expired.";
        public const string PASSWORD_RESET_CODE_CONFIRMED = "Reset Password Code confirmed successfully.";

        // =====================
        // 💊 Product & Inventory
        // =====================
        public const string PRODUCT_CREATED = "Product added successfully.";
        public const string PRODUCT_UPDATED = "Product updated successfully.";
        public const string PRODUCT_DELETED = "Product removed successfully.";
        public const string PRODUCT_NOT_FOUND = "Product not found.";
        public const string PRODUCT_OUT_OF_STOCK = "Product is currently out of stock.";
        public const string INVENTORY_UPDATED = "Inventory updated successfully.";
        public const string LOW_STOCK_WARNING = "Stock level is below the safe threshold.";

        // =====================
        // 🧾 Orders & Cart
        // =====================
        public const string ORDER_PLACED = "Order placed successfully.";
        public const string ORDER_UPDATED = "Order updated successfully.";
        public const string ORDER_CANCELLED = "Order cancelled successfully.";
        public const string ORDER_COMPLETED = "Order completed successfully.";
        public const string ORDER_NOT_FOUND = "Order not found.";
        public const string CART_UPDATED = "Cart updated successfully.";
        public const string ITEM_ADDED_TO_CART = "Item added to cart.";
        public const string ITEM_REMOVED_FROM_CART = "Item removed from cart.";
        public const string CART_EMPTY = "Cart is empty.";

        // =====================
        // 💳 Payments & Subscriptions
        // =====================
        public const string PAYMENT_PROCESSED = "Payment processed successfully.";
        public const string PAYMENT_FAILED = "Payment processing failed. Please check your payment details.";
        public const string REFUND_PROCESSED = "Refund processed successfully.";
        public const string REFUND_FAILED = "Refund request failed.";
        public const string SUBSCRIPTION_ACTIVATED = "Subscription activated successfully.";
        public const string SUBSCRIPTION_CANCELLED = "Subscription cancelled successfully.";
        public const string SUBSCRIPTION_FAILED = "Failed to activate subscription. Please try again.";

        // =====================
        // 💊 Prescriptions
        // =====================
        public const string PRESCRIPTION_UPLOADED = "Prescription uploaded successfully.";
        public const string PRESCRIPTION_NOT_FOUND = "Prescription not found.";
        public const string PRESCRIPTION_VALIDATED = "Prescription validated successfully.";
        public const string PRESCRIPTION_REJECTED = "Prescription validation failed.";


        // =====================
        // 📧 Email Operations
        // =====================
        public const string EMAIL_SENT = "Email sent successfully.";
        public const string EMAIL_NOT_SENT = "Failed to send email. Please try again later.";
        public const string VERIFICATION_SUCCESS = "Verification completed successfully.";
        public const string VERIFICATION_FAILED = "Verification failed. Please check the provided information.";
        public const string EMAIL_ALREADY_VERIFIED = "Email is already verified.";
        // =====================
        // 📁 File Upload / Media
        // =====================
        public const string FILE_UPLOADED = "File uploaded successfully.";
        public const string FILE_UPLOAD_FAILED = "File upload failed. Please try again.";
        public const string INVALID_FILE_TYPE = "Invalid file type. Please upload a supported file format.";
        public const string FILE_TOO_LARGE = "The uploaded file is too large. Please upload a smaller file.";
        public const string FILE_NOT_FOUND = "Requested file was not found.";



        // =====================
        // 📢 Notifications
        // =====================
        public const string NOTIFICATION_SENT = "Notification sent successfully.";
        public const string NOTIFICATION_FAILED = "Failed to send notification.";

        // =====================
        // ✉️ Email Subjects
        // =====================
        public const string SUBJECT_EMAIL_CONFIRMATION = "Confirm Your Smart Pharmacy Account";
        public const string SUBJECT_PASSWORD_RESET = "Reset Your Password - Smart Pharmacy";
        public const string SUBJECT_WELCOME = "Welcome to Smart Pharmacy!";
        public const string SUBJECT_ORDER_CONFIRMATION = "Order Confirmation - Smart Pharmacy";

        // =====================
        // ✉️ Email Templates
        // =====================
        public const string CONFIRMATIONEMAIL_TEMPLATE = @"
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            background-color: #f9f9f9;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 6px 20px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            border-bottom: 3px solid #007bff;
            padding-bottom: 10px;
        }}
        .header h1 {{
            color: #007bff;
            margin: 0;
        }}
        .content {{
            font-size: 16px;
            color: #333;
            margin-top: 20px;
            line-height: 1.6;
        }}
        .btn {{
            display: inline-block;
            margin-top: 25px;
            padding: 14px 30px;
            background-color: #007bff;
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 6px;
            font-weight: bold;
            text-align: center;
        }}
        .btn:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 40px;
            font-size: 12px;
            text-align: center;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Confirm Your Email</h1>
        </div>
        <div class='content'>
            <p>Hi {{UserName}},</p>
            <p>Thank you for joining <strong>SmartCare Pharmacy</strong>! Please confirm your email address by clicking the button below:</p>
            <p style='text-align:center;'>
                <a href='{{CallbackUrl}}' class='btn'>Confirm Email</a>
            </p>
            <p>If you didn’t create an account, you can safely ignore this email.</p>
        </div>
        <div class='footer'>
            &copy; {{Year}} SmartCare Pharmacy. All rights reserved.
        </div>
    </div>
</body>
</html>";



        public const string RESETPASSWORD_TEMPLATE = @"
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            background-color: #f9f9f9;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #fff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 6px 20px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            border-bottom: 3px solid #dc3545;
            padding-bottom: 10px;
        }}
        .header h1 {{
            color: #dc3545;
            margin: 0;
        }}
        .content {{
            font-size: 16px;
            color: #333;
            margin-top: 20px;
            line-height: 1.6;
        }}
        .code {{
            display: inline-block;
            background-color: #f0f0f0;
            padding: 12px 25px;
            border-radius: 5px;
            font-weight: bold;
            font-size: 20px;
            letter-spacing: 2px;
            color: #d9534f;
        }}
        .footer {{
            margin-top: 40px;
            font-size: 12px;
            text-align: center;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hi {{UserName}},</p>
            <p>We received a request to reset your password. Use the code below to complete your password reset:</p>
            <p style='text-align:center;'>
                <span class='code'>{{Code}}</span>
            </p>
            <p>If you didn’t request this, you can safely ignore this email.</p>
        </div>
        <div class='footer'>
            &copy; {{Year}} SmartCare Pharmacy. All rights reserved.
        </div>
    </div>
</body>
</html>";



        public const string WELCOMEEMAIL_TEMPLATE = @"
                                                                    <html>
                                                                    <head>
                                                                        <style>
                                                                            body {{
                                                                                font-family: Arial, sans-serif;
                                                                                background-color: #f4f4f4;
                                                                                margin: 0;
                                                                                padding: 0;
                                                                            }}
                                                                            .container {{
                                                                                max-width: 600px;
                                                                                margin: 40px auto;
                                                                                background-color: #ffffff;
                                                                                padding: 30px;
                                                                                border-radius: 8px;
                                                                                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                                                                            }}
                                                                            .header {{
                                                                                text-align: center;
                                                                                padding-bottom: 20px;
                                                                            }}
                                                                            .header h1 {{
                                                                                color: #28a745;
                                                                            }}
                                                                            .content {{
                                                                                font-size: 16px;
                                                                                line-height: 1.6;
                                                                                color: #555;
                                                                            }}
                                                                            .footer {{
                                                                                margin-top: 30px;
                                                                                font-size: 12px;
                                                                                text-align: center;
                                                                                color: #888;
                                                                            }}
                                                                        </style>
                                                                    </head>
                                                                    <body>
                                                                        <div class='container'>
                                                                            <div class='header'>
                                                                                <h1>Welcome to Smart Pharmacy</h1>
                                                                            </div>
                                                                            <div class='content'>
                                                                                <p>Hi {{UserName}},</p>
                                                                                <p>We’re thrilled to have you join <strong>Smart Pharmacy</strong>! You can now browse medicines, manage prescriptions, and enjoy convenient online orders.</p>
                                                                                <p>Explore our platform and discover the easiest way to manage your health and medications.</p>
                                                                                <p>Stay safe and healthy,</p>
                                                                                <p><strong>The Smart Pharmacy Team</strong></p>
                                                                            </div>
                                                                            <div class='footer'>
                                                                                &copy; {{Year}} Smart Pharmacy. All rights reserved.
                                                                            </div>
                                                                        </div>
                                                                    </body>
                                                                    </html>";


        public const string ORDERCONFIRMATION_TEMPLATE = @"
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            background-color: #f9f9f9;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #fff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 6px 20px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            border-bottom: 3px solid #17a2b8;
            padding-bottom: 10px;
        }}
        .header h1 {{
            color: #17a2b8;
            margin: 0;
        }}
        .content {{
            font-size: 16px;
            color: #333;
            margin-top: 20px;
            line-height: 1.6;
        }}
        .footer {{
            margin-top: 40px;
            font-size: 12px;
            text-align: center;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Confirmation</h1>
        </div>
        <div class='content'>
            <p>Hi {{UserName}},</p>
            <p>Thank you for your order! Your order <strong>#{{OrderId}}</strong> has been successfully placed and is now being processed.</p>
            <p>We’ll notify you once your items are shipped.</p>
            <p>Thank you for choosing <strong>SmartCare Pharmacy</strong>!</p>
        </div>
        <div class='footer'>
            &copy; {{Year}} SmartCare Pharmacy. All rights reserved.
        </div>
    </div>
</body>
</html>";


    }
}
