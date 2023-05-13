create schema `exam-security`;
use `exam-security`;

create table db_user (
	id bigint NOT NULL AUTO_INCREMENT,
    username varchar(255) NOT NULL UNIQUE,
    passwd varchar(255) NOT NULL,
    user_role varchar(64),
	PRIMARY KEY (id)
);