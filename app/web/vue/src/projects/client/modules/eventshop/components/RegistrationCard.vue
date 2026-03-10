<template>
  <div class="d-flex px-sm-15 px-3" v-if="!isLoading">
    <div class="card w-100 py-sm-8 py-0">
      <!-- header section -->
      <div class="modal-header border-0 px-sm-10 px-5 py-sm-5 py-3">
        <h2 class="fw-bold event-title">
          {{ details.title }}
        </h2>
      </div>
      <!-- content section -->
      <div class="modal-body px-sm-10 px-5 w-100 overflow-auto">
        <div class="w-100 notice-content" v-html="details.term"></div>
      </div>
      <!-- checkbox section -->
      <div
        class="d-flex flex-row align-items-center gap-2 mx-sm-10 mx-5 mb-sm-10 mb-5"
      >
        <input
          class=""
          :checked="checkedRef"
          :disabled="isRegistering"
          type="checkbox"
          name="user_management_read"
          @change="(e) => handleChange(e)"
        />
        <span>{{ $t("fields.iAgreeToEventRules") }}</span>
      </div>
      <!-- agree button -->
      <div class="d-flex align-items-center justify-content-end m-sm-10 m-5">
        <LoadingButton
          :is-loading="isRegistering"
          :disabled="!checkedRef || isRegistering"
          :save-title="$t('action.register')"
          :saved-title="$t('fields.processing')"
          class="btn btn-sm btn-primary d-flex align-items-center gap-2"
          @click.prevent="handleRegister"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import LoadingButton from "@/components/buttons/LoadingButton.vue";
import ShopService from "../services/ShopService";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";
const t = i18n.global.t;
const isLoading = ref(false);
const checkedRef = ref<boolean>(false);
const isRegistering = ref(false);
const handleChange = (e: Event) => {
  if (e instanceof Event) {
    const inputElement = e.target as HTMLInputElement;
    checkedRef.value = inputElement.checked;
  }
};
const details = ref(<any>[]);
const eventDetail = inject(ClientGlobalInjectionKeys.EVENT_SHOP_DETAIL);

const fetchData = async () => {
  isLoading.value = true;

  if (eventDetail !== undefined) {
    details.value = eventDetail.value;
    isLoading.value = false;
    return;
  }
  const response = await ShopService.queryEventByKey("EventShop");
  details.value = response;
  isLoading.value = false;
};

const handleRegister = async () => {
  isRegistering.value = true;

  try {
    await ShopService.registerEventByKey("EventShop");
    MsgPrompt.success(t("tip.registerSuccess")).then(() => {
      window.location.reload();
    });
  } catch (error) {
    MsgPrompt.error(error);
    console.log(error);
  }
  isRegistering.value = false;
};

onMounted(() => {
  fetchData();
});
</script>

<style lang="scss">
@media (max-width: 768px) {
  .event-title {
    font-size: 20px;
  }
}
</style>
