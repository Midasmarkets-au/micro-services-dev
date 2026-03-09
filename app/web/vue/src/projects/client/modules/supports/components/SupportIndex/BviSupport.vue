<template>
  <div :class="isMobile ? 'px-2' : ''">
    <div v-if="!isMobile" class="mb-5">
      <div class="bvi-bg card">
        <div
          class="d-flex justify-content-around gap-15 bvi_contact_box py-10 mx-auto w-lg-75"
        >
          <!--iphone-->
          <div v-if="contactConfig['phone'] != undefined">
            <div class="mb-5 d-flex justify-content-center">
              <inline-svg src="/images/icons/communication/phone.svg" />
            </div>
            <div>
              {{ contactConfig["phone"] }}
            </div>
          </div>
          <!--email-->
          <div>
            <div class="mb-5 d-flex justify-content-center">
              <inline-svg src="/images/icons/communication/mail.svg" />
            </div>
            <div>
              <template
                v-for="(item, key, index) in contactConfig['department']"
                :key="index"
              >
                <div class="d-flex justify-content-center">
                  <span class="me-1">{{ $t("title." + key) }}: </span>
                  <span class="email-link-color">{{ item }}</span>
                </div>
              </template>
            </div>
          </div>
          <!--zan-->
          <div>
            <div class="mb-5 d-flex justify-content-center">
              <inline-svg src="/images/icons/communication/thumbs-up.svg" />
            </div>
            <div>
              <div class="d-flex justify-content-center gap-4">
                <div
                  v-for="(item, key, index) in contactConfig['socialMedia']"
                  :key="index"
                >
                  <a :href="item.url">
                    <div class="">
                      <inline-svg :src="item.icon" />
                    </div>
                  </a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- <div class="position-relative mb-10 w-lg-75 mx-auto">
        <img src="/images/support/bvi_map.png" class="w-100 rounded-3" />
      </div> -->
    </div>

    <div v-if="isMobile" class="my-5">
      <div
        class="d-flex flex-column justify-content-center align-items-center pt-10 px-5"
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
        <img src="/images/support/bvi_cover_new.png" class="w-100 rounded-3" />
      </div>

      <div class="separator w-100 mx-auto"></div>
    </div>

    <div class="card mb-15 lg-mb-5 mt-5 px-5 px-lg-0">
      <div class="card-header">
        <div class="card-title">
          <h3 class="d-flex align-items-center mx-1 m-0 fw-bold">
            {{ $t("title.onlineEnquiry") }}
          </h3>
        </div>
      </div>
      <div class="col-lg-6 pe-lg-10 mt-5 mx-auto">
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
  console.log("config", config);
  contactConfig.value = JSON.parse(config.contactInfo?.value ?? "{}");
  console.log("contactConfig.value", contactConfig);
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
  background: rgba(255, 255, 255, 0.7);
}
.bvi-bg {
  width: 100%;
  height: 34.8rem;
  transition: opacity 1s ease-in-out;
  background-image: url("/images/support/bvi_cover_new.png");
  background-repeat: no-repeat;
  background-size: cover;
}
</style>
