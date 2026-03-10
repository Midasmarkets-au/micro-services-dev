<template>
  <div v-if="$cans(['Guest'])">
    <!--  Mobile Start  -->
    <div class="d-lg-none card mb-5" style="border-top: 4px solid #4196f0">
      <div class="card-body">
        <div class="d-flex mb-3 align-items-center">
          <inline-svg src="/images/icons/other/verify_sm.svg" />
          <h2 class="fw-bold text-gray-800 ms-1">
            {{ $t("tip.verifyYourAccount") }}
          </h2>
        </div>
        <div class="ms-7">
          <p class="text-muted fw-semibold fs-6">
            {{ $t("tip.verificationTip") }}
          </p>
          <router-link
            to="/verification"
            class="btn btn-secondary btn-lg btn-radius mt-5"
            >{{ $t("action.startNow") }}</router-link
          >
        </div>
      </div>
    </div>
    <!-- Mobile End    -->

    <div class="d-none d-lg-block card card-xl-stretch">
      <div
        class="card-body d-flex align-items-center justify-content-between flex-wrap"
      >
        <!-- <img src="/images/icons/other/verify.png" style="width: 72px" /> -->
        <div class="me-2 px-5" style="flex: 1">
          <h3 class="fw-bold banner-title leading-[4.28rem] text-5xl mt-6 mb-4">
            {{ $t("tip.verifyYourAccount") }}
          </h3>
          <p class="text-primary fs-2">
            {{ $t("tip.verificationTip") }}
          </p>
          <router-link
            to="/verification"
            class="btn btn-secondary btn-lg btn-radius custom-mb px-18"
            >{{ $t("action.startNow") }}</router-link
          >
        </div>
        <div class="mr-12 pr-4">
          <img src="/images/icons/other/verify.png" style="width: 140px" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts"></script>

<style scoped>
.custom-mb {
  margin-bottom: 3rem;
  margin-top: 3rem;
}

@media (max-width: 1512px) {
  .custom-mb {
    margin-bottom: 1.9rem;
    margin-top: 1.9rem;
  }
}
</style>
