<template>
  <div class="card card-xl-stretch mb-xl-8">
    <div class="card-header border-0 py-5">
      <h3 class="card-title align-items-start flex-column">
        <span class="card-label fw-bold fs-3 mb-1">{{
          $t("ib.newAccounts")
        }}</span>

        <span class="text-muted fw-semobold fs-7">{{
          $t("ib.tip.NewCreatedAccounts")
        }}</span>
      </h3>
    </div>
    <div class="card-body d-flex flex-column" v-if="isLoading">
      {{ $t("tip.loading") }}
    </div>
    <div class="card-body d-flex flex-column pt-0" v-else>
      <div class="mt-5">
        <div
          class="d-flex flex-stack mb-5"
          v-for="(account, index) in accounts"
          :key="index"
        >
          <UserInfo
            url="#"
            :avatar="account.avatar"
            :title="account.name"
            :sub="account.created_at"
            :name="account.name"
            class="me-2"
          />
          <div class="badge badge-light fw-semobold py-4 px-3">
            {{ account["account"] }}
          </div>
        </div>
        <div v-if="Object.keys(accounts).length === 7" class="text-end">
          <router-link to="/accounts/request" class="menu-link">{{
            $t("action.viewMore")
          }}</router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, inject, onMounted } from "vue";
import UserInfo from "@/components/UserInfo.vue";
import { apiProviderKey } from "@/core/plugins/providerKeys";
import { RouterLink } from "vue-router";

const isLoading = ref(true);
const api = inject(apiProviderKey);
const accounts = ref({});

const fetchData = async () => {
  isLoading.value = true;
  api["account.widgets.newRequest"]({})
    .then((res) => {
      accounts.value = res.data;
      isLoading.value = false;
    })
    .catch((err) => {
      console.log(err);
      // isLoading.value = false;
    });
};

onMounted(() => {
  fetchData();
});
</script>
