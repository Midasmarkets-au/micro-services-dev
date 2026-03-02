alter table acct."_PaymentService"
    add "CurrencyId" integer default -1 not null;

alter table acct."_PaymentService"
    add constraint "_PaymentService__Currency_Id_fk"
        foreign key ("CurrencyId") references acct."_Currency";
