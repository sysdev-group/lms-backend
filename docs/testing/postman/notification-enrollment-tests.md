# Notification and Enrollment API Postman Tests

## Overview

This document contains the Postman API testing results for the Notification and Enrollment endpoints implemented in the LMS Backend system.

Testing was performed using Postman with authenticated Bearer Token authorization. Automated Postman test scripts were added to each request, and successful API responses together with their test assertions were verified.

---

## Authentication

Authentication was performed through the login endpoint:

```text
POST /api/v1/auth/login
```

Request body:

```json
{
  "email": "admin@lms.com",
  "password": "Admin@123"
}
```

After successful login, the API returned a JWT access token. Bearer Token authentication was then used for all protected Notification and Enrollment API endpoints tested in Postman.

---

## Tested Endpoints

### 1. GET `/api/v1/notifications`

**Purpose:**  
Retrieve notifications for the authenticated user.

**Request Type:**  
`GET`

**Request Body:**  
None

**Post-response Postman Test Script:**

```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response success is true", function () {
    let jsonData = pm.response.json();
    pm.expect(jsonData.success).to.eql(true);
});
```

**Expected Result:**

- `200 OK`
- Notifications returned successfully
- Postman tests passed successfully `(2/2)`

---

### 2. PATCH `/api/v1/notifications/mark-all-read`

**Purpose:**  
Mark all notifications as read for the authenticated user.

**Request Type:**  
`PATCH`

**Request Body:**  
None

**Post-response Postman Test Script:**

```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Notifications marked as read", function () {
    let jsonData = pm.response.json();
    pm.expect(jsonData.success).to.eql(true);
});
```

**Expected Result:**

- `200 OK`
- Notifications marked as read successfully
- Postman tests passed successfully `(2/2)`

---

### 3. GET `/api/v1/enrollment/student/{studentId}`

**Purpose:**  
Retrieve enrollment records for a selected student.

**Request Type:**  
`GET`

**Student ID Used:**

```text
ed87eae7-a989-4b0d-842f-ac97a230f6bb
```

**Endpoint Used:**

```text
GET /api/v1/enrollment/student/ed87eae7-a989-4b0d-842f-ac97a230f6bb
```

**Request Body:**  
None

**Post-response Postman Test Script:**

```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Enrollment data returned", function () {
    let jsonData = pm.response.json();
    pm.expect(jsonData.success).to.eql(true);
});
```

**Expected Result:**

- `200 OK`
- Enrollment data returned successfully
- Postman tests passed successfully `(2/2)`

---

## Postman Test Scripts

Automated Postman test scripts were added after each request to confirm that:

- the API returned a successful `200 OK` status code
- the response body contained `success: true`
- each endpoint produced the expected successful result

These scripts provided repeatable verification of the tested Notification and Enrollment workflows.

---

## Test Results

All tested endpoints returned successful responses, and the automated Postman assertions passed successfully. The API responses matched the expected outcomes for each request.

The completed testing confirmed:

- successful `200 OK` responses
- successful notification retrieval and update operations
- successful retrieval of student enrollment data
- authenticated endpoint access functioning correctly through JWT Bearer Token authorization

---

## Screenshot Evidence

Screenshots were captured to show successful endpoint execution, request URLs, authenticated requests, response bodies, and Postman Test Results with passing assertions.

### GET Notifications

![GET Notifications](screenshots/get-notifications.png)

### PATCH Mark All Notifications Read

![PATCH Mark All Notifications Read](screenshots/mark-all-read.png)

### GET Student Enrollments

![GET Student Enrollments](screenshots/get-student-enrollments.png)

---

## Tools Used

- Postman
- ASP.NET Core LMS Backend API
- JWT Bearer Token Authentication
