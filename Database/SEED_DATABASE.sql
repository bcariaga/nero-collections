-- Create test data for user table

INSERT INTO nero."user" (id, username)
VALUES
('user1', 'alice'),
('user2', 'bob'),
('user3', 'charlie');

-- Create test data for collection table

INSERT INTO nero.collection (id, name)
VALUES
('collection1', 'My Favorite Manga'),
('collection2', 'Manga to Read'),
('collection3', 'Completed Manga');

-- Create test data for user_collection table

INSERT INTO nero.user_collection (user_id, collection_id)
VALUES
('user1', 'collection1'),
('user1', 'collection2'),
('user2', 'collection1'),
('user2', 'collection3'),
('user3', 'collection2');

-- Create test data for list table

INSERT INTO nero.list (id, name)
VALUES
('list1', 'Must-Read Manga'),
('list2', 'Underrated Manga Gems');

-- Create test data for collection_list table

INSERT INTO nero.collection_list (collection_id, list_id)
VALUES
('collection1', 'list1'),
('collection2', 'list1'),
('collection2', 'list2');

-- Create test data for book table

INSERT INTO nero.book (id, name)
VALUES
('book1', 'Berserk'),
('book2', 'Neon Genesis Evangelion'),
('book3', 'Slam Dunk');

-- Create test data for list_book table

INSERT INTO nero.list_book (list_id, book_id)
VALUES
('list1', 'book1'),
('list1', 'book2'),
('list2', 'book3');

-- Create test data for user_book_state table

INSERT INTO nero.user_book_state (user_id, book_id, state)
VALUES
('user1', 'book1', 'Completed'),
('user1', 'book2', 'In Progress'),
('user2', 'book1', 'Not Started'),
('user3', 'book3', 'Abandoned');
