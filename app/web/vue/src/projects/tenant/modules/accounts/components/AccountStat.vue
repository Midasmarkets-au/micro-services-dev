<template>
  <div>
    <div class="d-flex justify-content-center" v-if="isLoading">
      <LoadingRing />
    </div>
    <ul v-else>
      <li
        class="mb-3 pointArea p-3"
        v-for="(item, index) in items"
        :key="index"
        :class="
          item.uid == selectedAccount || selectedAccount == null ? '' : 'hide'
        "
        style="list-style: none"
      >
        <div class="row d-flex align-items-center">
          <div class="col-1">
            <span>
              <button
                v-if="!props.viewClient"
                class="btn btn-sm btn-icon btn-light-primary me-2"
                style="height: 20px"
                @click="showStatus(item.uid)"
              >
                <span v-if="item.uid == selectedAccount">Hide</span>
                <span v-else>View</span>
              </button>
            </span>
            <span v-if="item.uid != 0">{{ item.uid }}</span>
            <span v-else>Direct Client</span>
          </div>
          <div class="col-1">
            <span v-if="item.uid != 0">{{ item.group }}</span>
            <span v-else>--</span>
          </div>
          <div class="col-1">
            <div v-if="Object.keys(item.depositAmounts).length != 0">
              <span
                class="badge badge-primary"
                v-for="(value, key) in item.depositAmounts"
                :key="key"
              >
                <span class="me-1">{{ $t(`type.currency.${key}`) }}</span>
                <BalanceShow :currency-id="Number(key)" :balance="value" />
              </span>
            </div>
          </div>
          <div class="col-1">
            <div v-if="Object.keys(item.withdrawalAmounts).length != 0">
              <span
                class="badge badge-warning"
                style="color: black"
                v-for="(value, key) in item.withdrawalAmounts"
                :key="key"
              >
                <span class="me-1">{{ $t(`type.currency.${key}`) }}</span>
                <BalanceShow :currency-id="Number(key)" :balance="value" />
              </span>
            </div>
          </div>
          <div class="col-1">
            <div v-if="Object.keys(item.netAmounts).length != 0">
              <span
                class="badge"
                v-for="(value, key) in item.netAmounts"
                :key="key"
                :class="value > 0 ? 'badge-success' : 'badge-danger'"
              >
                <span class="me-1">{{ $t(`type.currency.${key}`) }}</span>
                <BalanceShow :currency-id="Number(key)" :balance="value" />
              </span>
            </div>
          </div>
          <div class="col-1">
            <div v-if="Object.keys(item.profitAmounts).length != 0">
              <span
                class="badge"
                v-for="(value, key) in item.profitAmounts"
                :key="key"
                :class="value > 0 ? 'badge-success' : 'badge-danger'"
              >
                <span class="me-1">{{ $t(`type.currency.${key}`) }}</span>
                <BalanceShow :currency-id="Number(key)" :balance="value" />
              </span>
            </div>
          </div>
          <div class="col-2">
            <div v-if="Object.keys(item.rebateAmounts).length != 0">
              <span
                class="badge badge-secondary me-2"
                v-for="(value, key) in item.rebateAmounts"
                :key="key"
              >
                <span class="me-1">{{ $t(`type.currency.${key}`) }}</span>
                <BalanceShow :currency-id="Number(key)" :balance="value" />
              </span>
              <ViewRebateStat
                v-if="!props.viewClient"
                :item="item"
                :from="props.from"
                :to="props.to"
              />
            </div>
          </div>
        </div>
      </li>

      <div class="mt-3" v-if="selectedAccount != null">
        <AccountStat
          :uid="selectedAccount == 0 ? props.uid : selectedAccount"
          :from="props.from"
          :to="props.to"
          :viewClient="viewClientAccount"
        ></AccountStat>
      </div>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import AccountStat from "./AccountStat.vue";
import AccountService from "../services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import BalanceShow from "@/components/BalanceShow.vue";
import ViewRebateStat from "@/projects/tenant/modules/accounts/components/modal/ViewRebateStat.vue";

const props = defineProps<{
  uid: any;
  from: any;
  to: any;
  viewClient: any;
}>();

const isLoading = ref(false);
const items = ref<any[]>([]);
const selectedAccount = ref<any>(null);
const viewClientAccount = ref(false);

const fetchData = async () => {
  isLoading.value = true;
  try {
    items.value = await AccountService.getChildAccountStat({
      uid: props.uid,
      from: props.from,
      to: props.to,
      viewClient: props.viewClient,
    });
  } catch (e) {
    MsgPrompt.error(e);
  }

  isLoading.value = false;
};

const showStatus = (_selectAccount: any) => {
  if (selectedAccount.value == _selectAccount) {
    selectedAccount.value = null;
    viewClientAccount.value = false;
  } else {
    selectedAccount.value = _selectAccount;
    if (_selectAccount == 0) {
      viewClientAccount.value = true;
    }
  }
};

onMounted(() => {
  fetchData();
});
</script>

<style scoped>
.hide {
  display: none;
}

.pointArea:hover {
  background-color: lightgray;
}
</style>
