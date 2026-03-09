<template>
  <div
    class="card-title-noicon d-flex justify-content-between align-items-center w-100 w-lg-auto"
    v-if="!isMobile"
  >
    <div>
      <el-select v-model="currentRole" :disabled="isLoading" class="w-125px">
        <el-option
          v-for="item in accountTypesSelection"
          :key="item.value"
          :label="item.label"
          :value="item.value"
        ></el-option>
      </el-select>
      <el-select
        v-model="multiLevel"
        :disabled="isLoading"
        class="ms-3 w-150px"
      >
        <el-option
          v-for="item in multiLevelOptions"
          :key="item.value"
          :label="item.label"
          :value="item.value"
        ></el-option>
      </el-select>
    </div>

    <div class="ms-3 d-flex align-items-center justify-content-center">
      <div
        v-if="currentRole == AccountRoleTypes.Client"
        class="d-flex align-items-center justify-content-center me-3 gap-3"
      >
        <el-select
          v-model="isDeposit"
          class="w-125px"
          :disabled="isLoading"
          @change="depositFetch(1)"
        >
          <el-option
            v-for="item in isDepositOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
        <el-date-picker
          v-model="startDate"
          type="date"
          :placeholder="t('fields.startDate')"
          class="w-150px"
          clearable
          :disabled="isLoading"
          value-format="YYYY-MM-DD"
        />
        <el-date-picker
          v-model="endDate"
          type="date"
          class="w-150px"
          :placeholder="t('fields.endDate')"
          clearable
          :disabled="isLoading"
          value-format="YYYY-MM-DD"
        />
      </div>

      <el-input
        clearable
        class="w-175px"
        :placeholder="$t('tip.searchKeyWords')"
        v-model="criteria.searchText"
        :disabled="isLoading"
      />
    </div>

    <div>
      <el-button
        class="ms-4"
        size="large"
        @click="search"
        :disabled="isLoading"
      >
        {{ $t("action.search") }}
      </el-button>
      <el-button size="large" @click="reset" :disabled="isLoading">
        {{ $t("action.reset") }}
      </el-button>
    </div>
  </div>
  <div v-if="isMobile" class="w-100">
    <div class="d-flex gap-3 w-100">
      <div class="w-100px">
        <el-select v-model="currentRole" :disabled="isLoading">
          <el-option
            v-for="item in accountTypesSelection"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
      </div>
      <div>
        <el-select
          v-model="multiLevel"
          :disabled="isLoading"
          class="ms-3 w-125px"
        >
          <el-option
            v-for="item in multiLevelOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
      </div>
      <div v-if="currentRole == AccountRoleTypes.Client">
        <el-select
          v-model="isDeposit"
          class="w-100px"
          :disabled="isLoading"
          @change="depositFetch(1)"
        >
          <el-option
            v-for="item in isDepositOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
      </div>
    </div>

    <div>
      <div
        class="d-flex gap-3 mt-5"
        v-if="currentRole == AccountRoleTypes.Client"
      >
        <el-date-picker
          v-model="startDate"
          type="date"
          :placeholder="t('fields.startDate')"
          class="w-150px"
          clearable
          :disabled="isLoading"
          value-format="YYYY-MM-DD"
        />
        <el-date-picker
          v-model="endDate"
          type="date"
          class="w-150px"
          :placeholder="t('fields.endDate')"
          clearable
          :disabled="isLoading"
          value-format="YYYY-MM-DD"
        />
      </div>

      <div class="d-flex mt-5">
        <el-input
          :disabled="isLoading"
          class="w-150px me-3"
          clearable
          :placeholder="$t('tip.searchKeyWords')"
          v-model="criteria.searchText"
        />
        <el-button size="large" @click="search" :disabled="isLoading">
          {{ $t("action.search") }}
        </el-button>
        <el-button size="large" @click="reset" :disabled="isLoading">
          {{ $t("action.reset") }}
        </el-button>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject, computed, watch } from "vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { isMobile } from "@/core/config/WindowConfig";
import moment from "moment";
import i18n from "@/core/plugins/i18n";
import { convertGMT } from "@/core/helpers/helpers";
const t = i18n.global.t;
const isLoading = inject<boolean>("isLoading");
const criteria = inject<any>("criteria");
const currentRole = inject<any>("currentRole");
const fetchData = inject<any>("fetchData");
const multiLevel = inject<any>("multiLevel");
const ibLevelTree = inject<any>("ibLevelTree");
const isDeposit = inject<any>("isDeposit");
const startDate = ref(<any>null);
const endDate = ref(<any>null);

const accountTypesSelection = ref(<any>[
  { label: t("title.all"), value: null },
  {
    label: t(`title.ib`),
    value: AccountRoleTypes.IB,
  },
  {
    label: t(`fields.client`),
    value: AccountRoleTypes.Client,
  },
  {
    label: t(`fields.sales`),
    value: AccountRoleTypes.Sales,
  },
]);

const multiLevelOptions = ref([
  { label: t("fields.directLevel"), value: false },
  { label: t("fields.allLevels"), value: true },
]);

const isDepositOptions = ref([
  { label: t("fields.all"), value: null },
  { label: t("fields.hasDeposit"), value: true },
  { label: t("fields.noDeposit"), value: false },
]);

// const title = computed(() => {
//   switch (currentRole.value) {
//     case AccountRoleTypes.Client:
//       return t("title.clientList");
//     case AccountRoleTypes.IB:
//       return t("title.ibList");
//     case AccountRoleTypes.Sales:
//       return t("title.salesList");
//     default:
//       return t("title.all");
//   }
// });

const search = async () => {
  if (ibLevelTree.value.length > 0) {
    currentRole.value = null;
  }
  await fetchData(1);

  criteria.value.from = null;
  criteria.value.to = null;
};

const depositFetch = async (_page: number) => {
  criteria.value.isActive = isDeposit.value;
  await fetchData(_page);
  criteria.value.isActive = null;
};

const reset = () => {
  initCriteria();
  startDate.value = null;
  endDate.value = null;
  isDeposit.value = null;
  fetchData(1);
};

const initCriteria = () => {
  var tempChildUid = criteria.value.parentAccountUid;
  criteria.value = {
    page: 1,
    size: 30,
    role: currentRole.value,
    multiLevel: multiLevel.value,
    sortField: "createdOn",
    sortFlag: true,
    searchText: null,
  };
  if (tempChildUid) {
    criteria.value.parentAccountUid = tempChildUid;
  }
};

watch(startDate, (val) => {
  if (val) {
    criteria.value.from = convertGMT(val, "start");
  }
});
watch(endDate, (val) => {
  if (val) {
    criteria.value.to = convertGMT(val, "end");
  }
});
</script>
