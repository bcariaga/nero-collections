create schema nero;


CREATE TABLE nero."user" (
    id VARCHAR(255) PRIMARY KEY,
    username VARCHAR(255) NOT NULL
);

CREATE TABLE nero.collection(
    id VARCHAR(255) PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

CREATE TABLE nero.user_collection(
    user_id VARCHAR(255) NOT NULL REFERENCES nero."user"(id),
    collection_id VARCHAR(255) NOT NULL REFERENCES nero.collection(id),
    PRIMARY KEY (user_id, collection_id)
);

CREATE TABLE nero.list(
    id VARCHAR(255) PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

CREATE TABLE nero.collection_list(
    collection_id VARCHAR(255) NOT NULL REFERENCES  nero.collection(id),
    list_id VARCHAR(255) NOT NULL REFERENCES nero.list(id),
    PRIMARY KEY (collection_id, list_id)
);

CREATE TABLE nero.book(
    id VARCHAR(255) PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

CREATE TABLE nero.list_book(
    list_id VARCHAR(255) NOT NULL REFERENCES nero.list(id),
    book_id VARCHAR(255) NOT NULL  REFERENCES  nero.book(id),
    PRIMARY KEY (list_id, book_id)
);

CREATE TABLE nero.user_book_state(
    user_id VARCHAR(255) NOT NULL REFERENCES nero."user"(id),
    book_id VARCHAR(255) NOT NULL REFERENCES nero.book(id),
    state VARCHAR(255) NOT NULL,
    PRIMARY KEY (user_id, book_id)
);