<template>
  <div class="card h-100">
    <div
      class="card-body d-flex flex-column align-items-center justify-content-center"
    >
      <div class="ib-avatar">
        <UserAvatar
          :avatar="user?.avatar"
          :name="user?.name"
          size="80px"
          class="me-3"
          side="client"
          v-if="user"
        />
      </div>

      <h3 class="fw-semibold fs-2 mt-3">{{ user.name }}</h3>

      <div class="ib-info">
        <div class="ib-code">
          <span class="mb-1">{{ $t("title.ibCode") }}</span>
          <span class="code mt-3">{{
            selectedIbAccount?.agentSelfGroupName
          }}</span>
        </div>
        <div class="current-account cursor-pointer">
          <div class="d-flex align-items-center">
            <span>{{ $t("title.currentAccount") }}</span>
            <el-icon
              class="ms-2 hover-effect"
              size="15"
              @click="updateAlias()"
              v-if="ibAccountsSelections.length > 1"
              ><Edit
            /></el-icon>
          </div>

          <el-select
            @change="changeAccount"
            class="account account-select mt-2"
            v-model="selectedIbAccount.uid"
          >
            <el-option
              v-for="(item, index) in ibAccountsSelections"
              :key="index"
              :label="
                item.alias === '' ||
                item.alias === null ||
                item.alias === undefined
                  ? item.label
                  : item.label + ' (' + item.alias + ')'
              "
              :value="item.value"
            />
          </el-select>
        </div>
      </div>

      <div class="ib-info">
        <div class="ib-code">
          <span class="mb-1">{{ $t("title.balance") }}</span>
          <span class="code">
            <BalanceShow
              :currency-id="accountDetails.currencyId"
              :balance="accountDetails.tradeAccount?.balanceInCents"
          /></span>
        </div>

        <div
          v-if="accountDetails.tradeAccount?.balanceInCents > 0"
          class="ib-code"
        >
          <span class="mb-1">{{ $t("action.action") }}</span>
          <span class="code text-center d-flex justify-content-between gap-2">
            <div class="widget-action-btn" @click="openWithdrawalModal">
              {{ $t("action.withdraw") }}
            </div>
            <div class="widget-action-btn" @click="openTransferToAccountPanel">
              {{ $t("action.transfer") }}
            </div>
          </span>
        </div>
      </div>
    </div>
  </div>
  <CreateWithdrawalModal ref="createWithdrawalModalRef" />
  <TransferToAccountForm ref="transferToAccountFormRef" />
  <EditAlias ref="EditAliasRef" @update="fetchData" />
</template>

<script setup lang="ts">
import UserAvatar from "@/components/UserAvatar.vue";
import { useStore } from "@/store";
import { ref, onMounted, nextTick } from "vue";
import { Actions } from "@/store/enums/StoreEnums";
import EditAlias from "@/projects/client/modules/ib/components/modal/EditAlias.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import CreateWithdrawalModal from "@/projects/client/components/funding/CreateWithdrawModal.vue";

import AccountService from "@/projects/client/modules/accounts/services/AccountService";
import {
  AccountRoleTypes,
  AccountStatusTypes,
} from "@/core/types/AccountInfos";
import TransferToAccountForm from "@/projects/client/modules/accounts/components/modal/TransferToAccountForm.vue";
import { Edit } from "@element-plus/icons-vue";
import { AgentAccount } from "@/projects/client/modules/ib/store/IbModule";
import { FundTypes } from "@/core/types/FundTypes";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { useRouter } from "vue-router";
const store = useStore();
const router = useRouter();
const isLoading = ref(true);
const user = store.state.AuthModule.user;
const transferAccountList = ref(Array<any>());
const createWithdrawalModalRef =
  ref<InstanceType<typeof CreateWithdrawalModal>>();
const transferToAccountFormRef =
  ref<InstanceType<typeof TransferToAccountForm>>();
const EditAliasRef = ref<any>(null);
const selectedIbAccount = ref(store.state.AgentModule.agentAccount);
const ibAccounts = ref(store.state.AgentModule.agentAccountList);
const ibAccountUids = ref(store.state.AuthModule.user.ibAccount);
const ibAccountsSelections = ref(Array<any>());
const accountDetails = ref({} as any);

const updateAlias = () => {
  EditAliasRef.value?.show(ibAccounts.value);
};
const changeAccount = async () => {
  if (!selectedIbAccount.value) {
    return;
  }
  isLoading.value = true;
  const selectedAccount = ibAccounts.value.find(
    (x) => x.uid === selectedIbAccount.value?.uid
  );
  await store.dispatch(Actions.PUT_IB_CURRENT_ACCOUNT, selectedAccount);
  isLoading.value = false;
  location.reload();
};

onMounted(async () => {
  try {
    const res = await AccountService.queryAccounts({
      uids: ibAccountUids.value,
    });
    ibAccounts.value = res.data.map(
      (x): AgentAccount => ({
        uid: x.uid,
        currencyId: x.currencyId as CurrencyTypes,
        fundType: x.fundType as FundTypes,
        role: x.role,
        type: x.type,
        name: x.name,
        siteId: x.siteId,
        hasLevelRule: x.hasLevelRule,
        salesGroupName: x.code,
        createdOn: x.createdOn,
        agentSelfGroupName: x.group,
        alias: x.alias,
      })
    );
    await store.dispatch(Actions.PUT_IB_ACCOUNTS, ibAccounts.value);

    ibAccountsSelections.value = ibAccounts.value.map((item) => ({
      label: item.uid,
      value: item.uid,
      alias: item.alias,
    }));
    accountDetails.value = res.data.find(
      (x) => x.uid === selectedIbAccount.value?.uid
    );

    if (!accountDetails.value) {
      window.localStorage.removeItem("ibCurrentAccount");
      window.localStorage.removeItem("ibAccounts");
      router.push("/").then(() => {
        window.location.reload();
      });
    }

    const res2 = await AccountService.queryAccounts({
      hasTradeAccount: true,
      status: AccountStatusTypes.Activate,
      roles: [
        AccountRoleTypes.Client,
        AccountRoleTypes.SuperAdmin,
        AccountRoleTypes.TenantAdmin,
        AccountRoleTypes.Wholesale,
        AccountRoleTypes.Guest,
      ],
    });
    const accountsList = res2.data;

    transferAccountList.value = accountsList.filter(
      (item) =>
        item?.currencyId === accountDetails?.value?.tradeAccount?.currencyId &&
        item.fundType == accountDetails.value.fundType &&
        item.tradeAccount.accountNumber !==
          accountDetails.value.tradeAccount.accountNumber
    );
  } catch (error) {
    console.log(error);
  }
});

const fetchData = async () => {
  const res = await AccountService.queryAccounts({
    uids: ibAccountUids.value,
  });

  var updatedIbAccounts = res.data.map(
    (x): AgentAccount => ({
      uid: x.uid,
      currencyId: x.currencyId as CurrencyTypes,
      fundType: x.fundType as FundTypes,
      role: x.role,
      type: x.type,
      name: x.name,
      siteId: x.siteId,
      hasLevelRule: x.hasLevelRule,
      salesGroupName: x.code,
      createdOn: x.createdOn,
      agentSelfGroupName: x.group,
      alias: x.alias,
    })
  );
  await store.dispatch(Actions.PUT_IB_ACCOUNTS, updatedIbAccounts);
  ibAccountsSelections.value = updatedIbAccounts.map((item) => ({
    label: item.uid,
    value: item.uid,
    alias: item.alias,
  }));
  ibAccounts.value = updatedIbAccounts;
};
const openTransferToAccountPanel = async () => {
  transferToAccountFormRef.value?.show(
    transferAccountList.value,
    accountDetails.value.tradeAccount.accountNumber,
    accountDetails.value.uid,
    accountDetails.value.tradeAccount.currencyId,
    accountDetails.value.fundType,
    accountDetails.value.tradeAccount.balance
  );
};

const openWithdrawalModal = () => {
  const isAccount = true;
  createWithdrawalModalRef.value?.show(isAccount, accountDetails.value);
};
</script>

<style scoped lang="scss">
.hover-effect:hover {
  color: #ffce00;
}
.card-body {
  padding: 1.25rem !important;
}
@media (max-width: 768px) {
  .card {
    height: auto !important;
  }
}
.ib-avatar {
  position: relative;
  width: 80px;
  height: 80px;

  .level-indicator {
    position: absolute;
    top: 0;
    right: 0;
    display: flex;
    flex-direction: row;
    justify-content: center;
    align-items: center;
    padding: 0px 5px;

    background: #070b0f;
    border: 3px solid #dab450;
    border-top-width: 2px;
    border-bottom-width: 2px;
    border-radius: 33px;

    font-size: 10px;
    color: #f7f1dc;

    transform: translate(30%, -30%);

    .level {
      font-weight: 700;
      font-size: 14px;
    }
  }
}

.exp-sider {
  position: relative;
  width: 237px;
  height: 22px;

  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 3px;

  background: #fcf3de;
  border-radius: 33px;
  color: #715329;
  font-size: 10px;
  font-weight: 500;

  .low-level,
  .high-level {
    display: flex;
    justify-content: center;
    align-items: center;
    top: 1px;
    box-sizing: border-box;
    width: 18px;
    height: 18px;
    border-radius: 50%;
    border: #e8ae56 3px solid;
    background-color: #f5c865;
  }

  .high-level {
    color: #c7ad82;
    border: #f1cc93 3px solid;
    background-color: #f7d795;
  }

  .points {
    width: 75%;
    height: 18px;
    padding-right: 10px;
    background: #f3c15a;
    border-radius: 33px;

    display: flex;
    justify-content: space-between;
    align-items: center;

    vertical-align: top;

    .full-scores {
      color: #bc9343;
    }
  }
}

.ib-info {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  width: 90%;
  margin-top: 20px;

  .ib-code,
  .current-account {
    font-weight: 400;
    font-size: 12px;
    color: #868d98;
    display: flex;
    flex-direction: column;
    gap: 5px;

    .code,
    .account {
      max-width: 190px;
      font-size: 16px;
      color: #3a3e44;
    }

    .account-style-btn {
      width: 125px;
    }
  }

  .widget-action-btn {
    min-width: 60px;
    padding: 5px;
    background: #0a46aa;
    border-radius: 8px;
    color: #fff;
    font-size: 12px;
    font-weight: 500;
    display: flex;
    justify-content: center;
    align-items: center;
    cursor: pointer;
  }
}
@media (min-width: 769px) and (max-width: 900px) {
  .ib-info {
    flex-direction: column !important;
    justify-content: center !important;
  }
}
/* :deep .el-input--suffix {
  border: 0px solid red;
  &:hover {
    border: 0;
  }
} */

// :deep .el-input__wrapper {
//   box-shadow: 0px 1px 0px rgba(0, 0, 0, 0.25);
// }

:deep .el-input__inner {
  border: 0;
  font-size: 16px;
  color: #3a3e44;
  //width: fit-content;
}
</style>
