<template>
  <SimpleForm ref="modalRef" :title="$t('title.2fa')" :is-loading="isLoading">
    <div class="d-flex flex-column align-items-center justify-content-center">
      <div class="qr-code mt-5 d-flex justify-content-center">
        <div ref="qrCodeUrl" class="qrcode"></div>
      </div>
      <div class="mt-5">{{ $t("tip.scanQrCodeUsingGoogleAuthenticator") }}</div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { nextTick, ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import QRCode from "qrcodejs2";

const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const qrCodeUrl = ref();

const show = async (authenticatorUri: string) => {
  modalRef.value?.show();
  isLoading.value = true;
  await nextTick();
  while (qrCodeUrl.value.firstChild) {
    qrCodeUrl.value.removeChild(qrCodeUrl.value.firstChild);
  }
  new QRCode(qrCodeUrl.value, {
    text: authenticatorUri,
    width: 128,
    height: 128,
    colorDark: "#000000",
    colorLight: "#ffffff",
    correctLevel: QRCode.CorrectLevel.H,
  });
  isLoading.value = false;
};

defineExpose({
  show,
  hide: () => modalRef.value?.hide(),
});
</script>

<style scoped lang="scss">
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
</style>
