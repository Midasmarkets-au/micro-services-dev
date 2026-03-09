<template>
  <div class="card h-100">
    <div class="card-header card-header-bottom">
      <div class="card-title-noicon fs-2">{{ $t("title.ibLink") }}</div>
      <div class="card-toolbar">
        <router-link to="/ib/iblink">
          <button
            class="btn btn-light btn-bordered btn-radius btn-xs d-flex align-items-center fs-9"
          >
            <span class="svg-icon svg-icon-7">
              <inline-svg src="/images/icons/ibCenter/ib_link.svg"
            /></span>
            {{ $t("title.ibLink") }}
          </button>
        </router-link>
      </div>
    </div>
    <!-- <div class="separator mx-auto" style="width: 85%"></div> -->
    <div class="card-body">
      <div class="ib-link-select">
        <el-select
          v-model="selectedInviteLink"
          size="large"
          :placeholder="$t('tip.chooseInviteLink')"
        >
          <el-option
            v-for="(item, index) in inviteLinks"
            :key="index"
            :label="item.label"
            :value="item.value"
          />
        </el-select>

        <div class="d-flex align-items-center gap-2 mt-1">
          <span class="svg-icon svg-icon-7">
            <!-- <svg
              width="16"
              height="16"
              viewBox="0 0 16 16"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <rect width="16" height="16" rx="8" fill="#009262" />
              <path
                d="M13.3333 4L5.99996 11.3333L2.66663 8"
                stroke="white"
                stroke-width="1.1"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg> -->
            <span class="svg-icon svg-icon-7">
              <inline-svg src="/images/icons/general/checked.svg"
            /></span>
          </span>
          <span style="font-size: 14px; color: #7c8fa2">
            {{ $t("tip.ready2Use") }}
          </span>
        </div>
      </div>

      <div class="qr-code mt-5 d-flex justify-content-center">
        <!--        {{ inviteLinks.length }}-->
        <div class="qrcode" ref="qrCodeUrl"></div>
        <NoDataCentralBox v-if="showNoDataBox" />
      </div>

      <div class="selected-link">
        <CopyBox
          class="select_input"
          ref="copyBoxRef"
          :val="selectedInviteLink"
          text-width="210px"
          :hasAni="true"
          :hasIcon="false"
        />
        <button @click="copyBoxRef.copy" class="btn btn-primary btn-xs">
          {{ $t("action.copy") }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref, onMounted, computed, watch } from "vue";
import CopyBox from "@/components/CopyBox.vue";
import { useStore } from "@/store";
import IbService from "../services/IbService";
import QRCode from "qrcodejs2";
import NoDataCentralBox from "@/components/NoDataCentralBox.vue";
const { locale } = useI18n();

const selectedInviteLink = ref("Please select a link");
const inviteLinks = ref(Array<any>());
const qrCodeUrl = ref();
const showNoDataBox = ref(false);
const copyBoxRef = ref<any>();

const store = useStore();
const siteId = computed(() => store.state.AuthModule.config.siteId);
var baseUrl = process.env.VUE_APP_BASE_URL;
if (siteId.value == 3 || siteId.value == 1) {
  baseUrl = process.env.VUE_APP_BASE_CDN_URL;
}

const generateLinkQrCode = () => {
  while (qrCodeUrl.value.firstChild) {
    qrCodeUrl.value.removeChild(qrCodeUrl.value.firstChild);
  }
  if (selectedInviteLink.value === "") {
    showNoDataBox.value = true;
    return;
  }
  showNoDataBox.value = false;
  new QRCode(qrCodeUrl.value, {
    text: selectedInviteLink.value,
    width: 128,
    height: 128,
    colorDark: "#000000",
    colorLight: "#ffffff",
    correctLevel: QRCode.CorrectLevel.H,
  });
};

watch(selectedInviteLink, () => generateLinkQrCode());

const fetchIbInviteLink = async () => {
  const ibLinksRes = await IbService.getIbLinks({ status: 0 });
  inviteLinks.value = ibLinksRes.data.map((link: any, idx: number) => ({
    label: link.name.length > 0 ? link.name : `Invite Link ${idx + 1}`,
    value: `${baseUrl}/sign-up?code=${link.code}&siteId=${store.state.AuthModule.config.siteId}&lang=${locale.value}`,
  }));
  selectedInviteLink.value =
    inviteLinks.value.length > 0 ? inviteLinks.value[0].value : "";
  // selectedInviteLink.value = "";
};

onMounted(async () => {
  await fetchIbInviteLink();
  generateLinkQrCode();
});
</script>

<style lang="scss" scoped>
.qrcode {
  display: inline-block;
  img {
    width: 132px;
    height: 132px;
    background-color: #fff; //设置白色背景色
    padding: 6px; // 利用padding的特性，挤出白边
    box-sizing: border-box;
  }
}
.ib-link-el-select {
  /* border: 1px solid #a8a8a8;
  width: 300px;
  height: 40px;
  padding: 0px;
  border-radius: 20px;
  overflow: hidden; */
  width: 310px;
  height: 40px;
  margin: -1px 0 0 -1px;
}

// .ib-link-select :deep .el-input__wrapper {
//   border-radius: 20px;
// }

.selected-link {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-left: 10px;
  font-size: 15px;
  color: #3a3e44;
  flex-direction: column;
  margin-top: 15px;
  border-radius: 8px;
  .select_input {
    border: #f2f4f7 1px solid;
    border-radius: 8px;
    padding: 4px;
  }
  padding-bottom: 10px;
  button {
    white-space: nowrap;
    margin-top: 11px;
    // width: 81px;
    // height: 36px;
    // margin-right: -3px;

    // /* primary/yellow */
    // border: 0;
    // border-radius: 8px;
    // background: #ffd400;
  }
}
// @media (min-width: 769px) and (max-width: 1512px) {
//   .selected-link {
//     flex-direction: column;
//     button {
//       margin-top: 11px;
//     }
//     .select_input {
//       padding: 0px;
//       border-radius: 8px;
//       padding: 4px;
//     }
//   }
// }
</style>
