CREATE TABLE Users (
    user_id INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(255) NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IP_Addresses (
    ip_id INT PRIMARY KEY AUTO_INCREMENT,
    ip_address VARCHAR(45) NOT NULL UNIQUE
);

CREATE TABLE HTTP_Methods (
    method_id INT PRIMARY KEY AUTO_INCREMENT,
    method_name VARCHAR(10) NOT NULL UNIQUE
);

CREATE TABLE HTTP_Status_Codes (
    status_code_id INT PRIMARY KEY AUTO_INCREMENT,
    status_code INT NOT NULL UNIQUE,
    description VARCHAR(255)
);

CREATE TABLE User_Agents (
    user_agent_id INT PRIMARY KEY AUTO_INCREMENT,
    user_agent TEXT NOT NULL
);

CREATE TABLE Pages (
    page_id INT PRIMARY KEY AUTO_INCREMENT,
    url VARCHAR(2083) NOT NULL UNIQUE
);

CREATE TABLE HTTP_Interactions (
    interaction_id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT,
    ip_id INT,
    method_id INT,
    status_code_id INT,
    user_agent_id INT,
    page_id INT,
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    request_body TEXT,
    response_body TEXT,
    response_time_ms INT,
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (ip_id) REFERENCES IP_Addresses(ip_id),
    FOREIGN KEY (method_id) REFERENCES HTTP_Methods(method_id),
    FOREIGN KEY (status_code_id) REFERENCES HTTP_Status_Codes(status_code_id),
    FOREIGN KEY (user_agent_id) REFERENCES User_Agents(user_agent_id),
    FOREIGN KEY (page_id) REFERENCES Pages(page_id)
);
