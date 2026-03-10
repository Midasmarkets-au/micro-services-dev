<template>
  <div class="overflow-auto sales-nav">
    <div class="sub-menu d-flex">
      <router-link to="/sales" class="sub-menu-item">{{
        $t("title.customer")
      }}</router-link>
      <router-link to="/sales/trade" class="sub-menu-item">{{
        $t("title.trade")
      }}</router-link>
      <router-link to="/sales/transaction" class="sub-menu-item">{{
        $t("title.transfer")
      }}</router-link>
      <router-link to="/sales/deposit" class="sub-menu-item">{{
        $t("title.deposit")
      }}</router-link>
      <router-link to="/sales/withdrawal" class="sub-menu-item">{{
        $t("title.withdrawal")
      }}</router-link>
      <router-link to="/sales/lead" class="sub-menu-item">{{
        $t("title.salesLeadSystem")
      }}</router-link>
      <router-link to="/sales/link" class="sub-menu-item active">{{
        $t("title.salesLink")
      }}</router-link>
    </div>
  </div>
  <div class="card p-4 p-lg-11">
    <div v-if="!isLoading" class="row row-cols-lg-3">
      <div
        class="col text-center"
        v-for="(item, index) in ibLinks"
        :key="index"
      >
        <h2 class="mb-7">
          {{
            item.name && item.name !== ""
              ? item.name
              : serviceTypeName[item.serviceType]
          }}
        </h2>
        <div class="sale-link-card">
          <div class="d-flex justify-content-center">
            <div
              class="d-flex justify-content-center align-items-center sale-link-code px-2 px-lg-6"
            >
              {{ item.code }}
            </div>
          </div>

          <div class="position-relative d-flex justify-content-center mt-13">
            <CopySalesLink :val="item.code" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import CopySalesLink from "@/components/CopySalesLink.vue";
import SalesService from "../services/SalesService";
import { useStore } from "@/store";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";

const store = useStore();
const ibLinks = ref([] as any);
const isLoading = ref(true);
const salesAccount = computed(() => store.state.SalesModule.salesAccount);

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SalesService.getIbLinks({ status: 0 });
    ibLinks.value = res.data;
  } catch (error) {
    // console.log(error);
  }
  isLoading.value = false;
};
const serviceTypeName = {
  1: "For IB",
  2: "For Agent",
  3: "For Client",
};
watch(salesAccount, (newVal, oldVal) => {
  if (newVal !== oldVal) {
    fetchData();
  }
});

onMounted(async () => {
  moibleNavScroller(".sales-nav", ".active");
  await fetchData();
});
</script>

<style scoped lang="scss">
.sub-menu {
  width: 100%;
  white-space: nowrap;
}
.sale-link-card {
  border: 1px dashed #ccc;
  padding: 40px;
  border-radius: 5px;
  width: 100%;
  margin: 0 auto;
  font-size: 18px;
  margin-bottom: 50px;
}
.sale-link-code {
  font-size: 48px;

  border: 1px solid gray;
  border-radius: 10px;
}

@media (max-width: 768px) {
  .sale-link-code {
    font-size: 32px;
  }
}
</style>
