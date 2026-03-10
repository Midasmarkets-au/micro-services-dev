<template>
  <div class="card">
    <div v-if="!isMobile" class="position-relative mb-5">
      <div class="bvi-bg"></div>
      <div
        class="position-relative d-flex justify-content-center gap-15 bvi_contact_box py-5 mx-auto my-10 w-lg-75"
      >
        <div v-if="contactConfig['offices'] != undefined">
          <div class="svg-icon svg-icon-1 mb-5 d-flex justify-content-center">
            <inline-svg src="/images/icons/communication/address.svg" />
          </div>

          <div
            v-for="(item, index) in contactConfig['offices']"
            :key="index"
            class="text-center"
          >
            {{ item }}
          </div>
        </div>
        <div v-if="contactConfig['phone'] != undefined">
          <div class="svg-icon svg-icon-1 mb-5 d-flex justify-content-center">
            <inline-svg src="/images/icons/communication/phone.svg" />
          </div>
          <div>
            {{ contactConfig["phone"] }}
          </div>
        </div>

        <div>
          <div class="svg-icon svg-icon-1 mb-5 d-flex justify-content-center">
            <inline-svg src="/images/icons/communication/mail.svg" />
          </div>
          <div>
            <template
              v-for="(item, key, index) in contactConfig['department']"
              :key="index"
            >
              <div class="d-flex justify-content-center">
                <span class="me-1">{{ $t("fields." + key) }}: </span>
                <span class="email-link-color">{{ item }}</span>
              </div>
            </template>
          </div>
        </div>

        <div>
          <div class="svg-icon svg-icon-1 mb-5 d-flex justify-content-center">
            <inline-svg src="/images/icons/communication/thumbs-up.svg" />
          </div>
          <div>
            <div class="d-flex justify-content-center gap-4">
              <div
                v-for="(item, key, index) in contactConfig['socialMedia']"
                :key="index"
              >
                <a :href="item.url">
                  <div class="svg-icon svg-icon-1">
                    <inline-svg :src="item.icon" />
                  </div>
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="position-relative mb-10 w-lg-75 mx-auto">
        <img src="/images/support/bvi_map.png" class="w-100 rounded-3" />
      </div>
    </div>

    <div v-if="isMobile" class="my-5">
      <div
        class="d-flex flex-column justify-content-center align-items-center pt-10 px-5"
      >
        <div v-if="contactConfig['offices'] != undefined" class="mb-5">
          <div class="svg-icon svg-icon-1 mb-5 d-flex justify-content-center">
            <inline-svg src="/images/icons/communication/address.svg" />
          </div>

          <div
            v-for="(item, index) in contactConfig['offices']"
            :key="index"
            class="text-center"
          >
            {{ item }}
          </div>
        </div>
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

      <div class="position-relative mb-10 w-lg-75 mx-auto">
        <img src="/images/support/bvi_map.png" class="w-100 rounded-3" />
      </div>

      <div class="separator w-100 mx-auto"></div>
    </div>

    <div class="row mb-15 lg-mb-5 mt-5 px-5 px-lg-0">
      <div class="col-lg-6 pe-lg-10 mx-auto">
        <OnlineEnquiry />
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
  console.log(contactConfig.value);
});
</script>
<style scoped lang="scss">
.email-link-color {
  color: #4196f0;
  cursor: pointer;
}
.social-media-icon {
  width: 80px;
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
.bvi_contact_box {
  border-radius: 8px;
  border: 1px solid var(--line, #efeff0);
  background: rgba(255, 255, 255, 0.95);
}
.bvi-bg {
  position: absolute;
  width: 100%;
  height: 100%;
  transition: opacity 1s ease-in-out;
  background-image: url("/images/support/bvi_cover.png");
  background-repeat: no-repeat;
  background-size: cover;
  background-position: 30% 50%;
  filter: blur(2.5px);
}
</style>
