# Notification and Enrollment API Postman Tests

## Purpose

This Postman collection verifies the LMS Notification and Enrollment API endpoints for retrieving notifications, marking notifications as read, and retrieving enrollment records for a selected student.

## Files

- `lms-notification-enrollment-tests.postman_collection.json`
- `lms-notification-enrollment-tests.postman_environment.json`

## Required Postman Environment Variables

| Variable | Example value | Notes |
| --- | --- | --- |
| `baseUrl` | `https://localhost:5001/api/v1` | API base URL used by every request. |
| `token` | `paste-valid-jwt-token-here` | Replace with a valid JWT access token. Do not commit real tokens. |
| `studentId` | `ed87eae7-a989-4b0d-842f-ac97a230f6bb` | Existing student id used by the enrollment lookup request. |

## How to Get a JWT Token From Login

1. Start the backend API.
2. In Postman, send a login request to `POST {{baseUrl}}/auth/login`.
3. Use the test account credentials required for the environment.
4. Copy the JWT access token returned in the login response.
5. Paste the token into the `token` environment variable.

Do not store real passwords or real JWT tokens in the collection or environment files.

## How to Run Each Test

1. Open Postman.
2. Import `lms-notification-enrollment-tests.postman_collection.json`.
3. Import `lms-notification-enrollment-tests.postman_environment.json`.
4. Select the imported environment.
5. Replace `token` with a valid JWT token.
6. Confirm that `studentId` contains an existing student id.
7. Run each request individually, or run the full collection.

Request definitions, Postman scripts, authorization configuration, and environment variables are included in the exported Postman collection and environment JSON files.

## Expected Results

| Request | Expected result |
| --- | --- |
| `GET {{baseUrl}}/notifications` | Returns `200 OK` with notifications for the authenticated user. Tests confirm a successful response. |
| `PATCH {{baseUrl}}/notifications/mark-all-read` | Returns `200 OK` after marking all notifications as read for the authenticated user. Tests confirm the successful update. |
| `GET {{baseUrl}}/enrollment/student/{{studentId}}` | Returns `200 OK` with enrollment records for the selected student. Tests confirm enrollment data is returned successfully. |

## Screenshot Checklist

Capture these screenshots for the report:

1. `GET /notifications` successful response.
2. `PATCH /notifications/mark-all-read` successful response.
3. `GET /enrollment/student/{studentId}` successful response.
4. Postman test results tab showing tests passed.
