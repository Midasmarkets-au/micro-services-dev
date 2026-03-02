alter table trd."_Group"
    rename column "AccountId" to "OwnerAccountId";

drop index trd."_Group_AccountId_Type_uindex";

create unique index "_Group_AccountId_Type_uindex"
    on trd."_Group" ("OwnerAccountId", "Type");

alter table trd."_Group"
    drop constraint "_Group__Account_Id_fk";

alter table trd."_Group"
    add constraint "_Group__Account_Id_fk"
        foreign key ("OwnerAccountId") references trd."_Account";
