<template>
  <div>
    <div v-if="!repAccount">{{ $t("action.noRepAccount") }}</div>
    <div v-else>
      <div class="sub-menu d-flex">
        <router-link to="/rep" class="sub-menu-item">{{
          $t("title.customer")
        }}</router-link>
        <router-link to="/rep/trade" class="sub-menu-item">{{
          $t("title.trade")
        }}</router-link>
        <router-link to="/rep/transaction" class="sub-menu-item">{{
          $t("title.transfer")
        }}</router-link>
        <router-link to="/rep/deposit" class="sub-menu-item active">{{
          $t("title.deposit")
        }}</router-link>
        <router-link to="/rep/withdrawal" class="sub-menu-item">{{
          $t("title.withdrawal")
        }}</router-link>
        <!-- <router-link to="/rep/lead" class="sub-menu-item">{{
          $t("title.repLeadSystem")
        }}</router-link> -->
      </div>

      <div class="card mb-2 px-5 round-bl-br">
        <RepDepositFilter
          ref="filterRef"
          :trigger="'button'"
          :filter-options="filterOptions"
          :service-handler="getFetchDataFunc"
          :default-criteria="defaultCriteria"
        />
      </div>
      <div class="card round-tl-tr">
        <div class="card-header">
          <div class="card-title">
            <h2 class="fw-semibold">
              {{
                $t("action.showing") +
                " " +
                (filterRef?.criteria.total ?? "0") +
                " " +
                $t("title.results")
              }}
            </h2>
          </div>
          <div class="card-toolbar">
            <button
              v-if="false && filterRef?.data.length !== 0"
              class="border-0 bg-transparent d-flex align-items-center"
            >
              <span class="me-2" style="color: #4196f0"> </span>
              <span class="svg-icon svg-icon-3">
                <inline-svg src="/images/icons/general/download.svg" />
              </span>
            </button>
          </div>
          <div class="card-toolbar"></div>
        </div>

        <div class="card-body">
          <table
            class="table align-middle table-row-bordered gy-5"
            id="table_accounts_requests"
          >
            <thead>
              <tr class="text-start gs-0">
                <th>{{ $t("fields.user") }}</th>
                <th>{{ $t("fields.email") }}</th>
                <th>{{ $t("fields.currency") }}</th>
                <th class="text-center">{{ $t("fields.targetAccount") }}</th>

                <th>{{ $t("fields.amount") }}</th>
                <th>{{ $t("fields.status") }}</th>
                <th>{{ $t("fields.createdOn") }}</th>
              </tr>
            </thead>
            <tbody v-if="filterRef?.isLoading">
              <LoadingRing />
            </tbody>
            <tbody
              v-else-if="!filterRef?.isLoading && filterRef?.data.length == 0"
            >
              <NoDataBox />
            </tbody>
            <tbody v-else>
              <tr v-for="(item, index) in filterRef?.getData()" :key="index">
                <td>
                  <div class="d-md-flex align-items-center">
                    <UserAvatar
                      :avatar="item.user.avatar"
                      :name="item.user.displayName"
                      size="40px"
                      class="me-3"
                      side="client"
                      rounded
                    />
                    <span>
                      {{
                        item.user.nativeName ||
                        item.user.displayName ||
                        `${item.user.firstName} ${item.user.lastNameName}`
                      }}
                    </span>
                  </div>
                </td>

                <td class="text-start">{{ item.user.email }}</td>

                <td class="text-start">
                  {{ $t(`type.currency.${item.currencyId}`) }}
                </td>

                <td class="text-center">
                  <div class="d-flex flex-column align-items-center">
                    <span class="text-gray-800">
                      No.
                      <span class="">
                        {{ item.targetTradeAccount.accountNumber }} ({{
                          $t(
                            `type.currency.${item.targetTradeAccount.currencyId}`
                          )
                        }})</span
                      >
                    </span>

                    <span class="fs-7">
                      Group:
                      <span class="">
                        {{ item.targetTradeAccount.group || "***" }}</span
                      >
                    </span>
                  </div>
                </td>

                <td class="text-start">
                  <BalanceShow
                    :balance="item.amount"
                    :currencyId="item.currencyId"
                  />
                </td>
                <td class="text-start">
                  <span
                    class="badge"
                    :class="`badge-${
                      {
                        [TransactionStateType.DepositCreated]: 'primary',
                        [TransactionStateType.DepositCompleted]: 'success',
                        [TransactionStateType.DepositTenantRejected]: 'danger',
                        [TransactionStateType.DepositTenantApproved]: 'warning',
                      }[item.stateId] ?? 'info'
                    }`"
                    >{{
                      $t(`type.transactionState.${item.stateId}`).replace(
                        /^deposit\s+/i,
                        ""
                      )
                    }}</span
                  >
                </td>

                <td class="text-start">
                  <TimeShow type="inFields" :date-iso-string="item.createdOn" />
                </td>
              </tr>
            </tbody>
          </table>

          <TableFooter
            @page-change="filterRef?.fetchData"
            :criteria="filterRef?.criteria"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { computed, ref, watch } from "vue";
import RepService from "@/projects/client/modules/rep/services/RepService";
import RepDepositFilter from "@/projects/client/modules/rep/components/RepDepositFilter.vue";
import { isMobile } from "@/core/config/WindowConfig";
import UserAvatar from "@/components/UserAvatar.vue";
import { TransactionStateType } from "@/core/types/StateInfos";
import TimeShow from "@/components/TimeShow.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import TableFooter from "@/components/TableFooter.vue";

const filterRef = ref<any>();
const store = useStore();
const repAccount = computed(() => store.state.RepModule.repAccount);

const filterOptions = ["size", "period", "accountNumber"];

const initialCriteria = ref<any>({
  page: 1,
  size: 10,
  isClosed: false,
  // openedFrom: moment().startOf("day").toISOString(),
  // openedTo: moment().endOf("day").toISOString(),
});
const defaultCriteria = ref<any>(initialCriteria.value);

const getFetchDataFunc = (criteria?: any) =>
  RepService.queryRepDeposit(criteria);
</script>

<style scoped></style>
