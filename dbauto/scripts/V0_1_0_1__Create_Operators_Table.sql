create table operators
(
    name     text primary key,
    auth_url text not null,
    enabled  integer default 1,
    auth_req_type smallint default 0,
    auth_res_type smallint default 0
);

create table operator_principals
(
    operator_name text primary key,
    client_id     text not null,
    client_secret text not null
);
    