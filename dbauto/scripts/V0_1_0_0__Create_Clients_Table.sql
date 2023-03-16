create table clients
(
    client_id     text primary key,
    client_secret text not null,
    enabled       integer default 1
);
    