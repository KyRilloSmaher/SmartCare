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
        public const string NOT_FOUND = "The requested resource was not found.";
        public const string INVALID_PAGINATION_PARAMETERS = " Page Number and Page Size Should be Greater than 0";
        public const string NO_DATA_FOUND = "No data found.";
        public const string INVALID_DATE_RANGE = "INVALID DATE RANGE";
        public const string OPERATION_TIMEOUT = "Time Out !";
        public const string RESERVATION_INVALID = "RESERVATION INVALID";
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
        public const string PHONE_ALREADY_EXISTS = "The phone number is already registered.";
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
        public const string ADDRESS_IS_REQUIRED = "Address is Required ";

        // =====================
        // 💊 Product & Inventory
        // =====================
        public const string PRODUCT_CREATED = "Product added successfully.";
        public const string PRODUCT_UPDATED = "Product updated successfully.";
        public const string PRODUCT_DELETED = "Product removed successfully.";
        public const string PRODUCT_NOT_FOUND = "Product not found.";
        public const string PRODUCT_OUT_OF_STOCK = "Product is currently out of stock.";
        public const string INVENTORY_UPDATED = "Inventory updated successfully.";
        public const string INVENTORY_NOT_FOUND = "Inventory not found.";
        public const string LOW_STOCK_WARNING = "Stock level is below the safe threshold.";

        // =====================
        // 🧾 Orders & Cart
        // =====================
        public const string ORDER_PLACED = "Order placed successfully.";
        public const string ORDER_UPDATED = "Order updated successfully.";
        public const string ORDER_CANCELLED = "Order cancelled successfully.";
        public const string ORDER_COMPLETED = "Order completed successfully.";
        public const string ORDER_NOT_FOUND = "Order not found.";
        public const string INVALID_ORDER_STATUS = "INVALID ORDER STATUS";
        public const string CART_UPDATED = "Cart updated successfully.";
        public const string ITEM_ADDED_TO_CART = "Item added to cart.";
        public const string ITEM_REMOVED_FROM_CART = "Item removed from cart.";
        public const string PRODUCT_ALREADY_IN_CART = "This Product is already Exist In your cart ";
        public const string CART_EMPTY = "Cart is empty.";
        public const string CART_CREATED = "CART CREATED";
        public const string CART_ALREADY_EXISTS = "An active cart already exists for this user.";
        public const string INSUFFICIENT_STOCK = "Insufficient stock for the requested product.";
        public const string ADDED_TO_CART = "Item added to cart successfully.";
        public const string CART_CLEARED = "Cart Cleared ";
        public const string CART_NOT_FOUND = "Cart Not Found In Database ";
        public const string CART_ITEM_NOT_EXIST = "This Product Not Exists In Your Cart !";
        public const string RESERVATION_FAILED = "Failed to create reservation for the product.";
        public const string INVALID_ORDER_TYPE = "INVALID ORDER TYPE";
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

        // =======================
        //  RATES
        // =======================
        public const string RATE_NOT_FOUND = "No Rate Found by This Id";
        public const string RATE_ADDED_SUCCESSFULLY = "Rate Added Successfully";
        public const string FAVOURITE_ALREADY_EXISTS = "Already Added Before";
        public const string RATE_ALREADY_EXISTS = "Rate already Exits";

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

        // ==========
        // Stores
        // ==========
        public const string STORE_CREATED = "Store created successfully.";
        public const string STORE_UPDATED = "Store updated successfully.";
        public const string STORE_DELETED = "Store deleted successfully.";
        public const string STORE_NOT_FOUND = "Store not found.";
        public const string STORE_ID_REQUIRED = "STORE id is Reuired ";
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
                                                                  <body style='margin:0; padding:20px; background-color:#f9f9f9; font-family:Segoe UI,Arial,sans-serif;'>
                                                                    <div style='max-width:600px; margin:auto; background-color:#ffffff; padding:30px; border-radius:10px; box-shadow:0 6px 20px rgba(0,0,0,0.1);'>
                                                                      <div style='text-align:center; border-bottom:3px solid #007bff; padding-bottom:10px;'>
                                                                        <h1 style='color:#007bff; margin:0;'>Confirm Your Email</h1>
                                                                      </div>
                                                                      <div style='font-size:16px; color:#333; margin-top:20px; line-height:1.6;'>
                                                                        <p>Hi {{UserName}},</p>
                                                                        <p>Thank you for joining <strong>SmartCare Pharmacy</strong>! Please confirm your email address by clicking the button below:</p>
                                                                        <div style='text-align:center; margin-top:25px;'>
                                                                          <a href='{{CallbackUrl}}' style='display:inline-block; padding:14px 30px; background-color:#007bff; color:#ffffff; text-decoration:none; border-radius:6px; font-weight:bold;'>Confirm Email</a>
                                                                        </div>
                                                                        <p style='margin-top:20px;'>If you didn’t create an account, you can safely ignore this email.</p>
                                                                      </div>
                                                                      <div style='margin-top:40px; font-size:12px; text-align:center; color:#888;'>&copy; {{Year}} SmartCare Pharmacy. All rights reserved.</div>
                                                                    </div>
                                                                  </body>
                                                                </html>";


        public const string RESETPASSWORD_TEMPLATE = @"
                                                        <html>
                                                          <body style='margin:0; padding:20px; background-color:#f9f9f9; font-family:Segoe UI,Arial,sans-serif;'>
                                                            <div style='max-width:600px; margin:auto; background-color:#ffffff; padding:30px; border-radius:10px; box-shadow:0 6px 20px rgba(0,0,0,0.1);'>
                                                              <div style='text-align:center; border-bottom:3px solid #dc3545; padding-bottom:10px;'>
                                                                <h1 style='color:#dc3545; margin:0;'>Password Reset Request</h1>
                                                              </div>
                                                              <div style='font-size:16px; color:#333; margin-top:20px; line-height:1.6;'>
                                                                <p>Hi {{UserName}},</p>
                                                                <p>We received a request to reset your password. Use the code below to complete your password reset:</p>
                                                                <div style='text-align:center; margin-top:25px;'>
                                                                  <span style='display:inline-block; background-color:#f8f9fa; padding:12px 25px; border-radius:5px; font-weight:bold; font-size:20px; letter-spacing:2px; color:#dc3545;'>{{Code}}</span>
                                                                </div>
                                                                <p style='margin-top:20px;'>If you didn’t request this, you can safely ignore this email.</p>
                                                              </div>
                                                              <div style='margin-top:40px; font-size:12px; text-align:center; color:#888;'>&copy; {{Year}} SmartCare Pharmacy. All rights reserved.</div>
                                                            </div>
                                                          </body>
                                                        </html>";

        public const string WELCOMEEMAIL_TEMPLATE = @"
                                                        <html>
                                                          <body style='margin:0; padding:20px; background-color:#f4f4f4; font-family:Arial, sans-serif;'>
                                                            <div style='max-width:600px; margin:40px auto; background-color:#ffffff; padding:30px; border-radius:8px; box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
                                                              <div style='text-align:center; padding-bottom:20px;'>
                                                                <h1 style='color:#28a745; margin:0;'>Welcome to SmartCare Pharmacy</h1>
                                                              </div>
                                                              <div style='font-size:16px; line-height:1.6; color:#555;'>
                                                                <p>Hi {{UserName}},</p>
                                                                <p>We’re thrilled to have you join <strong>SmartCare Pharmacy</strong>! You can now browse medicines, manage prescriptions, and enjoy convenient online orders.</p>
                                                                <p>Explore our platform and discover the easiest way to manage your health and medications.</p>
                                                                <p>Stay safe and healthy,</p>
                                                                <p><strong>The SmartCare Pharmacy Team</strong></p>
                                                              </div>
                                                              <div style='margin-top:30px; font-size:12px; text-align:center; color:#888;'>&copy; {{Year}} SmartCare Pharmacy. All rights reserved.</div>
                                                            </div>
                                                          </body>
                                                        </html>";

        public const string ORDERCONFIRMATION_TEMPLATE = @"
                                                            <html>
                                                              <body style='margin:0; padding:20px; background-color:#f9f9f9; font-family:Segoe UI,Arial,sans-serif;'>
                                                                <div style='max-width:600px; margin:auto; background-color:#ffffff; padding:30px; border-radius:10px; box-shadow:0 6px 20px rgba(0,0,0,0.1);'>
                                                                  <div style='text-align:center; border-bottom:3px solid #17a2b8; padding-bottom:10px;'>
                                                                    <h1 style='color:#17a2b8; margin:0;'>Order Confirmation</h1>
                                                                  </div>
                                                                  <div style='font-size:16px; color:#333; margin-top:20px; line-height:1.6;'>
                                                                    <p>Hi {{UserName}},</p>
                                                                    <p>Thank you for your order! Your order <strong>#{{OrderId}}</strong> has been successfully placed and is now being processed.</p>
                                                                    <p>We’ll notify you once your items are shipped.</p>
                                                                    <p>Thank you for choosing <strong>SmartCare Pharmacy</strong>!</p>
                                                                  </div>
                                                                  <div style='margin-top:40px; font-size:12px; text-align:center; color:#888;'>&copy; {{Year}} SmartCare Pharmacy. All rights reserved.</div>
                                                                </div>
                                                              </body>
                                                            </html>";

        // ====================================
        // HTML PAGES 
        // ====================================
        public const string PaymentSuccessPage =
        @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Payment Successful | SmartCare Pharmacy</title>
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css"">
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        
        body {
            background: linear-gradient(135deg, #f0f7ff 0%, #e1eeff 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }
        
        .container {
            max-width: 500px;
            width: 100%;
            background-color: white;
            border-radius: 16px;
            box-shadow: 0 10px 30px rgba(0, 82, 204, 0.15);
            overflow: hidden;
            text-align: center;
        }
        
        .header {
            background: linear-gradient(135deg, #1e6fd9 0%, #0a4da2 100%);
            padding: 30px 20px;
            color: white;
        }
        
        .logo {
            max-width: 180px;
            height: auto;
            margin-bottom: 15px;
            border-radius: 8px;
        }
        
        .header h1 {
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 8px;
        }
        
        .header p {
            opacity: 0.9;
            font-size: 16px;
        }
        
        .content {
            padding: 40px 30px;
        }
        
        .success-icon {
            width: 100px;
            height: 100px;
            background-color: #e8f4ff;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
            margin: 0 auto 25px;
            color: #1e6fd9;
            font-size: 42px;
        }
        
        .content h2 {
            color: #1a5cb3;
            font-size: 26px;
            margin-bottom: 15px;
            font-weight: 600;
        }
        
        .content p {
            color: #4a5568;
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 25px;
        }
        
        .app-return {
            background-color: #f7fbff;
            border-radius: 12px;
            padding: 20px;
            margin: 25px 0;
            border: 1px solid #e1eeff;
        }
        
        .app-return h3 {
            color: #1a5cb3;
            margin-bottom: 15px;
            font-size: 18px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
        }
        
        .app-return p {
            font-size: 15px;
            margin-bottom: 0;
        }
        
        .btn {
            padding: 14px 28px;
            border-radius: 8px;
            font-weight: 600;
            font-size: 16px;
            cursor: pointer;
            transition: all 0.3s ease;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            background-color: #1e6fd9;
            color: white;
            border: none;
            margin-top: 10px;
        }
        
        .btn:hover {
            background-color: #155bb5;
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(30, 111, 217, 0.3);
        }
        
        .footer {
            background-color: #f7fbff;
            padding: 20px;
            text-align: center;
            color: #4a5568;
            font-size: 14px;
            border-top: 1px solid #e1eeff;
        }
        
        .footer a {
            color: #1e6fd9;
            text-decoration: none;
        }
        
        @media (max-width: 480px) {
            .container {
                border-radius: 12px;
            }
            
            .header {
                padding: 25px 15px;
            }
            
            .content {
                padding: 30px 20px;
            }
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <img src=""https://res.cloudinary.com/dwahjkgbk/image/upload/v1763550528/photo_2025-11-19_13-08-12_wvzvek.jpg"" alt=""SmartCare Pharmacy Logo"" class=""logo"">
            <h1>SmartCare Pharmacy</h1>
            <p>Your Health, Our Priority</p>
        </div>
        
        <div class=""content"">
            <div class=""success-icon"">
                <i class=""fas fa-check""></i>
            </div>
            
            <h2>Payment Successful!</h2>
            <p>Your payment has been processed successfully and your order has been placed.</p>
            
            <div class=""app-return"">
                <h3><i class=""fas fa-mobile-alt""></i> Return to Mobile App</h3>
                <p>Please return to the SmartCare Pharmacy mobile app to continue.</p>
                <a href=""#"" class=""btn"">
                    <i class=""fas fa-arrow-left""></i> Back to App
                </a>
            </div>
            
            <p>You will receive a confirmation with your order details shortly.</p>
        </div>
        
        <div class=""footer"">
            <p>Need help? <a href=""#"">Contact our support team</a></p>
            <p>&copy; 2025 SmartCare Pharmacy. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        public const string PaymentFailurePage =
        @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Payment Failed | SmartCare Pharmacy</title>
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css"">
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        
        body {
            background: linear-gradient(135deg, #fff5f5 0%, #ffecec 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }
        
        .container {
            max-width: 500px;
            width: 100%;
            background-color: white;
            border-radius: 16px;
            box-shadow: 0 10px 30px rgba(204, 0, 0, 0.1);
            overflow: hidden;
            text-align: center;
        }
        
        .header {
            background: linear-gradient(135deg, #d93a1e 0%, #a20a0a 100%);
            padding: 30px 20px;
            color: white;
        }
        
        .logo {
            max-width: 180px;
            height: auto;
            margin-bottom: 15px;
            border-radius: 8px;
        }
        
        .header h1 {
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 8px;
        }
        
        .header p {
            opacity: 0.9;
            font-size: 16px;
        }
        
        .content {
            padding: 40px 30px;
        }
        
        .failure-icon {
            width: 100px;
            height: 100px;
            background-color: #ffe8e6;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
            margin: 0 auto 25px;
            color: #d93a1e;
            font-size: 42px;
        }
        
        .content h2 {
            color: #b31a1a;
            font-size: 26px;
            margin-bottom: 15px;
            font-weight: 600;
        }
        
        .content p {
            color: #4a5568;
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 25px;
        }
        
        .app-return {
            background-color: #fff5f5;
            border-radius: 12px;
            padding: 20px;
            margin: 25px 0;
            border: 1px solid #ffd6d6;
        }
        
        .app-return h3 {
            color: #b31a1a;
            margin-bottom: 15px;
            font-size: 18px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
        }
        
        .app-return p {
            font-size: 15px;
            margin-bottom: 0;
        }
        
        .btn {
            padding: 14px 28px;
            border-radius: 8px;
            font-weight: 600;
            font-size: 16px;
            cursor: pointer;
            transition: all 0.3s ease;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            background-color: #d93a1e;
            color: white;
            border: none;
            margin-top: 10px;
        }
        
        .btn:hover {
            background-color: #b31a1a;
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(217, 58, 30, 0.3);
        }
        
        .support-info {
            margin-top: 20px;
            padding: 15px;
            background-color: #f7fbff;
            border-radius: 8px;
            border-left: 4px solid #1e6fd9;
        }
        
        .support-info p {
            font-size: 14px;
            margin-bottom: 0;
            color: #4a5568;
        }
        
        .support-info a {
            color: #1e6fd9;
            text-decoration: none;
            font-weight: 600;
        }
        
        .footer {
            background-color: #f7fbff;
            padding: 20px;
            text-align: center;
            color: #4a5568;
            font-size: 14px;
            border-top: 1px solid #e1eeff;
        }
        
        .footer a {
            color: #1e6fd9;
            text-decoration: none;
        }
        
        @media (max-width: 480px) {
            .container {
                border-radius: 12px;
            }
            
            .header {
                padding: 25px 15px;
            }
            
            .content {
                padding: 30px 20px;
            }
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <img src=""https://res.cloudinary.com/dwahjkgbk/image/upload/v1763550528/photo_2025-11-19_13-08-12_wvzvek.jpg"" alt=""SmartCare Pharmacy Logo"" class=""logo"">
            <h1>SmartCare Pharmacy</h1>
            <p>Your Health, Our Priority</p>
        </div>
        
        <div class=""content"">
            <div class=""failure-icon"">
                <i class=""fas fa-times""></i>
            </div>
            
            <h2>Payment Failed</h2>
            <p>We're sorry, but your payment could not be processed at this time.</p>
            
            <div class=""app-return"">
                <h3><i class=""fas fa-mobile-alt""></i> Return to Mobile App</h3>
                <p>Please return to the SmartCare Pharmacy mobile app to try again or use a different payment method.</p>
                <a href=""#"" class=""btn"">
                    <i class=""fas fa-arrow-left""></i> Back to App
                </a>
            </div>
            
            <div class=""support-info"">
                <p>If you continue to experience issues, please <a href=""#"">contact our support team</a> for assistance.</p>
            </div>
        </div>
        
        <div class=""footer"">
            <p>Need help? <a href=""#"">Contact our support team</a></p>
            <p>&copy; 2025 SmartCare Pharmacy. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}
