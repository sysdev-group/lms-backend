# Course API Postman Tests

## Purpose

This Postman collection verifies the LMS Course API endpoints for listing courses, retrieving one course, creating a valid course, and rejecting invalid course input.

## Files

- `lms-course-api-tests.postman_collection.json`
- `lms-course-api-tests.postman_environment.json`

## Required Postman Environment Variables

| Variable | Example value | Notes |
| --- | --- | --- |
| `baseUrl` | `https://localhost:5001/api/v1` | API base URL used by every request. |
| `token` | `paste-valid-jwt-token-here` | Replace with a valid JWT access token. Do not commit real tokens. |
| `courseId` | `paste-existing-course-id-here` | Replace with an existing course id before running the valid GET-by-id test. The POST success test updates this value when a course is created. |

## How to Get a JWT Token From Login

1. Start the backend API.
2. In Postman, send a login request to `POST {{baseUrl}}/auth/login`.
3. Use a valid test account email and password.
4. Copy the access token returned in the login response.
5. Paste the token into the `token` environment variable.

Do not store real passwords or real JWT tokens in the collection or environment files.

## How to Run Each Test

1. Open Postman.
2. Import `lms-course-api-tests.postman_collection.json`.
3. Import `lms-course-api-tests.postman_environment.json`.
4. Select the `LMS Course API Local` environment.
5. Replace `token` with a valid JWT token.
6. Replace `courseId` with an existing course id.
7. In `POST valid course`, replace `PASTE_VALID_LECTURER_GUID` and `PASTE_VALID_SEMESTER_GUID` with valid ids from the database.
8. Run each request individually, or run the full `LMS Course API Tests` collection.

## Expected Results

| Request | Expected result |
| --- | --- |
| `GET {{baseUrl}}/courses` | Returns `200 OK` with a course list. Tests check the response envelope fields: `success`, `data`, `message`, `errors`, and list or pagination data. |
| `GET {{baseUrl}}/courses/{{courseId}}` | Returns `200 OK` with one course object. Tests check the envelope and course fields such as `id`, `code`, and `title`. |
| `GET {{baseUrl}}/courses/00000000-0000-0000-0000-000000000000` | Returns `404 Not Found`. Tests check the error envelope when a JSON body is returned. |
| `POST {{baseUrl}}/courses` with valid body | Returns `201 Created`. Tests check the success envelope and save the created course id into `courseId` when possible. |
| `POST {{baseUrl}}/courses` with invalid body | Returns `400 Bad Request` with validation details. Tests check for envelope errors or standard validation error fields. |

## Screenshot Checklist

Capture these screenshots for the report:

1. `GET /courses` successful response.
2. `GET /courses/{id}` successful response.
3. `GET /courses/{invalidId}` `404 Not Found` response.
4. `POST /courses` successful creation response.
5. `POST /courses` validation error response.
6. Postman test results tab showing tests passed.
