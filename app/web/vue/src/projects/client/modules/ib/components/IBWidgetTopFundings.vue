<template>
  <div class="h-100 position-relative">
    <div
      class="w-100 h-200px d-flex flex-column align-items-center pt-12"
      style="background-color: #edf4ff; border-radius: 16px"
    >
      <span class="text-gray"> {{ $t("title.top5Fundings") }} </span>
      <h2 class="font-medium mt-1 fs-2">{{ $t("title.funding") }}</h2>
    </div>
    <div
      class="content h-325px position-absolute"
      style="
        left: 50%;
        transform: translate(-50%, -23%);
        width: 100%;
        background-color: #ffffff;
        border-bottom: 1px solid #e4e6ef;
        border-left: 1px solid #e4e6ef;
        border-right: 1px solid #e4e6ef;
        border-bottom-left-radius: 20px;
        border-bottom-right-radius: 20px;
      "
    >
      <table
        v-if="isLoading || latestDeposit.length === 0"
        class="table align-middle fs-6 gy-5 h-100"
      >
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && latestDeposit.length === 0">
          <NoDataBox />
        </tbody>
      </table>

      <div v-else class="d-flex flex-column pt-1">
        <div
          class="flex-fill d-flex align-items-center justify-content-between px-4 py-5"
          v-for="(item, index) in latestDeposit"
          :key="index"
        >
          <div class="d-flex align-items-center">
            <label class="fs-5 me-3"> {{ index + 1 }}</label>

            <div class="customer-avatar">
              <UserAvatar
                :avatar="item.avatar"
                :name="item.name"
                size="28px"
                class="me-3"
                side="client"
              />
            </div>

            <span class="text-primary ms-2">
              {{
                item.name.substring(0, 11) +
                (item.name.length > 11 ? "..." : "")
              }}
            </span>
            <span
              class="ms-3 custome-badge"
              :class="{
                'ib-badge':
                  item.role === AccountRoleTypes.IB ||
                  item.role === AccountRoleTypes.Sales,
                'client-badge': item.role === AccountRoleTypes.Client,
              }"
              ><template
                v-if="
                  [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
                    item?.role
                  )
                "
              >
                {{ $t(`fields.ib`) }}
              </template>
              <template v-else>
                {{ $t(`type.accountRole.${item?.role}`) }}</template
              >
            </span>
          </div>

          <div>
            <BalanceShow
              class="fw-normal fs-4"
              :balance="item.amount"
              :currency-id="item.currencyId"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import IbRepostService from "../services/IbReportService";
import { useStore } from "@/store";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import BalanceShow from "@/components/BalanceShow.vue";

const isLoading = ref(true);
const latestDeposit = ref(Array<any>());
const store = useStore();

onMounted(async () => {
  const res = await IbRepostService.getLatestDeposits();
  latestDeposit.value = res.map((item: any) => ({
    name: item.user.firstName + " " + item.user.lastName,
    avatar: item.user.avatar,
    role: item.account.role,
    amount: item.amount,
    currencyId: item.currencyId,
  }));
  // latestDeposit.value = latestDeposit.value.filter((_, idx) => idx < 3);

  isLoading.value = false;
  // setTimeout(() => {
  //   latestDeposit.value = [
  //     {
  //       avatar: "b78afec1-7c35-4665-8b19-8d863159d346",
  //       name: "Nguyen Van",
  //       role: AccountRoleTypes.IB,
  //       amount: 1000,
  //       currencyId: 840,
  //     },
  //     {
  //       avatar: "b78afec1-7c35-4665-8b19-8d863159d346",
  //       name: "Eason Feng",
  //       role: AccountRoleTypes.Client,
  //       amount: 1000,
  //       currencyId: 840,
  //     },
  //     {
  //       avatar: "b78afec1-7c35-4665-8b19-8d863159d346",
  //       name: "Zhang San",
  //       role: AccountRoleTypes.Client,
  //       amount: 1000,
  //       currencyId: 840,
  //     },
  //     {
  //       avatar: "b78afec1-7c35-4665-8b19-8d863159d346",
  //       name: "Li Si",
  //       role: AccountRoleTypes.Client,
  //       amount: 1000,
  //       currencyId: 840,
  //     },
  //     {
  //       avatar: "b78afec1-7c35-4665-8b19-8d863159d346",
  //       name: "Nguyen Van",
  //       role: AccountRoleTypes.Client,
  //       amount: 1000,
  //       currencyId: 840,
  //     },
  //   ];
  //   isLoading.value = false;
  // }, 1000);
  // try {
  //   const res = await IbService.getLatestDepositReport(agentAccount.value, 5);
  //   console.log(res);
  // } catch (error) {
  //   console.log(error);
  // }
});
</script>

<style lang="scss" scoped>
.customer-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  overflow: hidden;
}

.custome-badge {
  padding: 1px 10px;
  border-radius: 10px;
  font-size: 12px;
}

.client-badge {
  color: #0a46aa;
  background: rgba(138, 195, 255, 0.18);
}

.ib-badge {
  color: #ff9900;
  background: rgba(255, 212, 0, 0.2);
}
</style>
