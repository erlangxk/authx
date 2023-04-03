create table clients
(
    client_id     text primary key,
    client_secret text not null,
    enabled       integer default 1
);

create table operators
(
    name     text primary key,
    auth_url text not null,
    enabled  integer default 1,
    pipeline smallint default 0,
    params text
);
    