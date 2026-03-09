<template>
  <div class="card card-flush mb-5">
    <div class="card-header">
      <div class="card-title">
        <h3 class="d-flex align-items-center mx-1 m-0 fw-bold">
          {{ $t("title.ReferralLink") }}
        </h3>
      </div>
    </div>
    <div class="card-body">
      <div class="selected-link">
        <CopyBox
          class=""
          ref="copyBoxRef"
          :val="selectedInviteLink"
          :text-width="isMobile ? '220px' : '450px'"
          :hasAni="true"
          :hasIcon="false"
        />
        <button @click="copyBoxRef.copy">{{ $t("action.copy") }}</button>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import CopyBox from "@/components/CopyBox.vue";
import { isMobile } from "@/core/config/WindowConfig";
import { useStore } from "@/store";
import { useI18n } from "vue-i18n";
import AccountService from "../services/AccountService";

const props = defineProps({
  currentAccount: {
    type: Object,
    required: true,
  },
});
const selectedInviteLink = ref("Please select a link");
const copyBoxRef = ref<any>();
const store = useStore();
const { locale } = useI18n();
const link = ref(props.currentAccount.selfReferCode);
const fetchIbInviteLink = async () => {
  if (!link.value || link.value == "") {
    selectedInviteLink.value = "Link is not available";
  } else {
    const baseUrl = "https://bvi.midasmkts.com";
    selectedInviteLink.value = `${baseUrl}/sign-up?code=${link.value}&siteId=${store.state.AuthModule.config.siteId}&lang=${locale.value}`;
  }
};

onMounted(async () => {
  await fetchIbInviteLink();
});
</script>
<style scoped lang="scss">
.selected-link {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-left: 10px;
  height: 36px;
  font-size: 14px;
  color: #7c8fa2;
  border: #e0e2ec 1px solid;
  border-radius: 5px;
  max-width: 550px;

  button {
    width: 81px;
    height: 36px;
    margin-right: -3px;

    /* primary/yellow */
    border: 0;
    border-radius: 8px;
    background: #ffd400;
  }

  @media (min-width: 769px) and (max-width: 1081px) {
    .selected-link {
      flex-direction: column;

      button {
        margin-top: 11px;
      }
    }
  }
}
</style>
