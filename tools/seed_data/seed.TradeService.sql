-- auto-generated definition
create table "_TradeService"
(
    "Id"                     integer                                                not null
        constraint trade_services_pkey
            primary key,
    "Platform"               smallint                 default '0'::smallint         not null,
    "Priority"               integer                  default 1                     not null,
    "IsAllowAccountCreation" smallint                 default 1                     not null,
    "CreatedOn"              timestamp with time zone default now()                 not null,
    "UpdatedOn"              timestamp with time zone default now()                 not null,
    "Name"                   varchar(50)                                            not null,
    "Description"            varchar(128)             default ''::character varying not null,
    "Configuration"          text                     default ''::text              not null
);

alter table "_TradeService"
    owner to bcrpro;

grant select on "_TradeService" to dev_read;

INSERT INTO trd."_TradeService" ("Id", "Platform", "Priority", "IsAllowAccountCreation", "CreatedOn", "UpdatedOn", "Name", "Description", "Configuration") VALUES (10, 20, 1, 1, '2023-11-10 05:15:54.263446 +00:00', '2023-11-10 05:15:54.263370 +00:00', 'BCRCO-Real', 'BCRCO-Real', e'{
  "Api": {
    "Host": "192.168.1.1",
    "Port": 7777,
    "User": null,
    "Password": null
  },
  "Database": {
    "UserTableName": "xxxxxxxx",
    "TradeTableName": "xxxxxxxx",
    "Username": "xxxxxxxx",
    "Password": "xxxxxxxx",
    "Host": "xxxxxxxx.amazonaws.com",
    "Port": 7777,
    "Database": "xxxxxxxx",
    "ConvertZeroDateTime": null,
    "ConnectionString": "xxxxxxxx"
  },
  "Groups": [
    "xxxxxxxx",
    "xxxxxxxx"
  ],
  "Leverages": null,
  "AccountPrefixes": {
    "USD": 33000000,
    "AUD": 35000000,
    "EUR": 37000000,
    "GBP": 34000000,
    "DEFAULT": 35000000
  },
  "DefaultGroup": {},
  "ConnectionString": null
}');
INSERT INTO trd."_TradeService" ("Id", "Platform", "Priority", "IsAllowAccountCreation", "CreatedOn", "UpdatedOn", "Name", "Description", "Configuration") VALUES (31, 31, 1, 1, '2023-11-10 05:15:54.263547 +00:00', '2023-11-10 05:15:54.263547 +00:00', 'BCR-MT5', 'BCR-MT5 Demo', e'{
    "Api": {
        "Host": "https://xx.xx.xx.xx",
        "User": "7777",
        "Password": "xxxxxx"
    },
    "Groups": [
        "xxxxxx\\\\xxxxxx",
        "xxxxxx\\\\xxxxxx"
    ],
    "DefaultGroup":{
        "4_840": "demo\\\\xxxxxx",
        "4_36": "demo\\\\xxxxxx",
        "6_840": "demo\\\\xxxxxx",
        "6_36": "demo\\\\xxxxxx",
        "11_840": "demo\\\\xxxxxx",
        "11_36": "demo\\\\xxxxxx",
        "default": "demo\\\\xxxxxx"
    },
    "Database": {
        "Username": "xxxxxx",
        "Password": "xxxxxx",
        "Host": "xxxxxx.xxxxxx.xxxxxx.rds.amazonaws.com",
        "Port": 7777,
        "Database": "xxxxxx",
        "UserTableName": "xxxxxx",
        "TradeTableName": "xxxxxx"
    }
}');
INSERT INTO trd."_TradeService" ("Id", "Platform", "Priority", "IsAllowAccountCreation", "CreatedOn", "UpdatedOn", "Name", "Description", "Configuration") VALUES (30, 30, 1, 1, '2023-11-10 05:15:54.263546 +00:00', '2023-11-10 05:15:54.263546 +00:00', 'BCR-MT5', 'BCR-MT5 Real', e'{
  "Api": {
    "Host": "https://xx.xx.xx.xx",
    "Port": 7777,
    "User": "xxxxxx",
    "Password": "xxxxxxx"
  },
  "Database": {
    "UserTableName": "xxxxxx",
    "TradeTableName": "xxxxxx",
    "Username": "xxxxxx",
    "Password": "xxxxxx",
    "Host": "xxxxxx.rds.amazonaws.com",
    "Port": 7777,
    "Database": "xxxxxx",
    "ConvertZeroDateTime": null,
    "ConnectionString": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
  },
  "Groups": [
    "xxxxxx\\\\xxxxxx\\\\xxxxxx\\\\xxxxxx",
    "xxxxxx\\\\xxxxxx\\\\xxxxxx\\\\xxxxxx"
  ],
  "Leverages": null,
  "AccountPrefixes": {
    "USD": 32000000,
    "AUD": 36000000,
    "EUR": 37000000,
    "GBP": 34000000,
    "DEFAULT": 36000000
  },
  "DefaultGroup": {},
  "ConnectionString": null
}');
