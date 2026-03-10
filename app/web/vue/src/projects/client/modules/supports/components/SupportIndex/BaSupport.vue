<template>
  <div class="card">
    <div class="p-lg-17">
      <div
        v-if="!isMobile"
        class="d-flex flex-column flex-lg-row justify-content-center borders py-5 mb-10"
      >
        <div
          v-if="contactConfig['phone'] != undefined"
          class="col-md-2 py-10 px-5 border-right"
        >
          <div class="svg-icon svg-icon-1 mb-5">
            <inline-svg src="/images/icons/communication/phone.svg" />
          </div>
          <div>
            {{ contactConfig["phone"] }}
          </div>
        </div>

        <div class="col-md-4 py-10 px-lg-5 border-right">
          <div class="svg-icon svg-icon-1 mb-5">
            <inline-svg src="/images/icons/communication/mail.svg" />
          </div>
          <div>
            <template
              v-for="(item, key, index) in contactConfig['department']"
              :key="index"
            >
              <div class="d-flex">
                <label class="me-1">{{ $t("title." + key) }}: </label>
                <span class="email-link-color">{{ item }}</span>
              </div>
            </template>
          </div>
        </div>

        <div class="col-md-4 p-10 px-5 border-right">
          <div class="svg-icon svg-icon-1 mb-5">
            <inline-svg src="/images/icons/communication/location.svg" />
          </div>
          <div
            class="mb-3"
            v-for="(item, key, index) in contactConfig['offices']"
            :key="index"
          >
            {{ $t("title." + key) }}:<br />
            {{ item }}
          </div>
        </div>

        <div class="col-md-2 ps-5 p-lg-15">
          <div :class="[isMobile ? 'd-flex gap-10' : 'social-media-icon']">
            <a
              :href="item.url"
              v-for="(item, key, index) in contactConfig['socialMedia']"
              :key="index"
            >
              <div class="svg-icon svg-icon-1">
                <inline-svg :src="item.icon" />
              </div>
            </a>
          </div>
        </div>
      </div>

      <div v-if="isMobile" class="mb-5">
        <div
          class="d-flex flex-column justify-content-center align-items-center pt-10 px-3"
        >
          <div class="svg-icon svg-icon-1 mb-3">
            <inline-svg src="/images/icons/communication/phone.svg" />
          </div>
          <div class="mb-8" v-if="contactConfig['phone'] != undefined">
            {{ contactConfig["phone"] }}
          </div>

          <div class="svg-icon svg-icon-1 mb-3">
            <inline-svg src="/images/icons/communication/mail.svg" />
          </div>

          <p
            class="m-0"
            v-for="(item, key, index) in contactConfig['department']"
            :key="index"
          >
            <label class="me-1">{{ $t("title." + key) }}: </label>
            <span class="email-link-color">{{ item }}</span>
          </p>

          <div class="separator w-75 mx-auto mt-5"></div>

          <div
            class="d-flex flex-column justify-content-center align-items-center py-5 px-5 text-center"
          >
            <div
              class="mb-3"
              v-for="(item, key, index) in contactConfig['offices']"
              :key="index"
            >
              {{ $t("title." + key) }}:<br />
              {{ item }}
            </div>
          </div>

          <div class="separator w-75 mx-auto"></div>

          <div class="d-flex gap-5 mt-8 mb-5">
            <a
              :href="item.url"
              v-for="(item, key, index) in contactConfig['socialMedia']"
              :key="index"
            >
              <div class="svg-icon svg-icon-1">
                <inline-svg :src="item.icon" />
              </div>
            </a>
          </div>
        </div>
      </div>

      <div class="mb-10 w-lg-75 mx-auto h-400px">
        <div class="h-100 w-100">
          <iframe
            :src="contactConfig['googleMap']"
            width="100%"
            height="100%"
            style="border: 0"
            allowfullscreen="true"
            loading="lazy"
            referrerpolicy="no-referrer-when-downgrade"
            v-if="contactConfig['googleMap'] != undefined"
          ></iframe>
        </div>
      </div>

      <div class="separator w-100 mx-auto"></div>

      <div class="row mb-15 lg-mb-5 mt-5 px-5 px-lg-0">
        <div class="col-lg-6 pe-lg-10 mx-auto">
          <OnlineEnquiry />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import OnlineEnquiry from "./OnlineEnquiry.vue";
import { isMobile } from "@/core/config/WindowConfig";
import store from "@/store";

const contactConfig = ref({});

onMounted(() => {
  let config = store.state.AuthModule.config;
  contactConfig.value = JSON.parse(config.contactInfo?.value ?? "{}");
});
</script>
<style scoped lang="scss">
.email-link-color {
  color: #4196f0;
  cursor: pointer;
}
.social-media-icon {
  width: 80px;
  height: 80px;
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: 1fr 1fr;
}
.border-right {
  border-right: solid 1px #e4e6ef;
  color: #4d4d4d;
}
.borders {
  border-top: solid 1px #e4e6ef;
  color: #4d4d4d;
  border-bottom: solid 1px #e4e6ef;
}

@media (max-width: 768px) {
  .border-right {
    border-right: none;
  }
}
</style>
