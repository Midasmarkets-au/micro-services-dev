select *,
       COALESCE((select ("PrevBalance" + "Amount")
                 from acct."_WalletTransaction" wt
                 where "CreatedOn" < '2023-08-23'
                   and wt."WalletId" = w."Id"
                 order by "Id" desc
                 limit 1), 0) as "BalanceSnapshot"
from acct."_Wallet" w
order by "BalanceSnapshot" desc
limit 100;
