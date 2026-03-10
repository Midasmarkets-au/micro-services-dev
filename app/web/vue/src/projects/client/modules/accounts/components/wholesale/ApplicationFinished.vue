<template>
  <div class="card wholesale-process-card">
    <!-- ================================================================== Wealth Threshold Application -->
    <div v-if="props.selectedMethod == '1'" class="card-body p-3 p-md-0">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("wholesale.finishTitle_1") }}
        </h2>
      </div>

      <div class="fs-4 mt-3">
        {{ $t("wholesale.finishContent_1") }}
      </div>
      <div class="fs-4 mt-9">
        {{ $t("wholesale.finishContent_2_1")
        }}<a href="mailto:wholesale@midasmkts.com" class="mm"
          >wholesale@midasmkts.com</a
        >.{{ $t("wholesale.finishContent_2_2") }}
      </div>

      <hr
        class="mt-9"
        style="
          border: none;
          border-top: 1px dotted gray;
          color: #fff;
          background-color: #fff;
          height: 1px;
          width: 100%;
        "
      />

      <div class="fv-row mb-10 ms-3" style="font-size: 16px">
        <div class="row">
          <h4 class="mt-5 mb-9">
            {{ $t("wholesale.finishContent_3") }}
          </h4>

          <div class="ms-md-9">
            <div class="d-flex">
              <div class="bullet"></div>
              <div>{{ $t("wholesale.finishContent_4") }}</div>
            </div>
            <div class="mt-5 mb-5">{{ $t("wholesale.or") }}</div>
            <div class="d-flex">
              <div class="bullet"></div>
              <div>{{ $t("wholesale.finishContent_5") }}</div>
            </div>
          </div>

          <div class="mt-11 fs-3 fw-bold">
            {{ $t("wholesale.finishTitle_2") }}
          </div>
          <div class="mt-6">
            {{ $t("wholesale.finishContent_6_1") }}
            <a target="_blank" :href="fileLink"
              ><strong>{{ $t("wholesale.startLink_1") }}</strong></a
            >{{ $t("wholesale.finishContent_6_2") }}
          </div>
          <div class="mt-6">
            {{ $t("wholesale.finishContent_7_1")
            }}<a href="#"><strong>wholesale@midasmkts.com</strong></a
            >.{{ $t("wholesale.finishContent_7_2") }}
          </div>
          <div class="mt-6" v-html="$t('wholesale.finishContent_8')"></div>
        </div>
      </div>
    </div>

    <!-- ================================================================== Sophisticated Investor Test -->
    <div
      v-else
      class="w-100 verify-card"
      style="
        max-width: 880px;
        margin: auto;
        border-top: none !important;
        border-top-right-radius: 0 !important;
        border-top-left-radius: 0 !important;
      "
    >
      <!-- <div
        class="mx-auto w-250px h-300px shadow-sm d-flex flex-column justify-content-center align-items-center gap-5 p-5"
      >
        <img src="/images/success.svg" alt="" class="w-75" />
        <h2 style="color: #0a46aa; font-size: 32px">
          {{ $t("status.success") + "!" }}
        </h2>
        <p class="text-center">
          {{ $t("tip.verificationFinishedWaitApproval") }}
        </p>
      </div> -->
      <div class="w-100 card mt-6">
        <div
          class="mx-auto h-500px d-flex flex-column justify-content-center align-items-center gap-5 p-5"
        >
          <img src="/images/success.svg" alt="" />
          <h2 style="color: #0a46aa; font-size: 32px">
            {{ $t("status.success") + "!" }}
          </h2>
          <p class="text-center">
            {{ $t("tip.verificationFinishedWaitApproval") }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { wholeSaleDocs, getWholeSaleDocs } from "@/core/data/bcrDocs";

var fileLink = getWholeSaleDocs(
  wholeSaleDocs.certificateByQualifiedAccountant.title
);

const props = defineProps<{
  selectedMethod?: string;
}>();
</script>
