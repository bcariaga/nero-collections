CREATE TABLE collections (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE
);

CREATE TABLE book_lists (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    collection_id INTEGER NOT NULL,
    FOREIGN KEY (collection_id) REFERENCES collections(id)
);

CREATE TABLE books (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    state VARCHAR(255) NOT NULL,
    book_list_id INTEGER NOT NULL,
    FOREIGN KEY (book_list_id) REFERENCES book_lists(id)
);


