<template>
  <div class="modal-dialog modal-dialog-centered mw-1000px">
    <div class="modal-content">
      <div class="modal-header" id="kt_modal_new_address_header">
        <h6 class="fs-5">{{ title }}</h6>

        <div data-bs-dismiss="modal">
          <span class="svg-icon svg-icon-1">
            <inline-svg src="/images/icons/arrows/arr061.svg" />
          </span>
        </div>
      </div>

      <div style="max-height: 80vh; overflow: auto">
        <div v-if="isLoading" class="d-flex justify-content-center">
          <LoadingRing />
        </div>
        <div
          v-else-if="!isLoading && data.length == 0"
          class="d-flex justify-content-center"
        >
          <NoDataBox />
        </div>
        <div v-else>
          <el-card v-for="(item, index) in data" :key="index" class="m-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                {{ item.name
                }}<i
                  class="fa-solid fa-pen-to-square ms-2"
                  style="cursor: pointer"
                  @click="editCode(item)"
                ></i>
              </div>
              <div>{{ item.code }}</div>
            </div>
            <div class="d-flex justify-content-between align-items-center mt-2">
              <div class="d-flex">
                <span v-if="item.serviceType == ReferralServiceType.Client">
                  <span
                    v-for="(acc, index) in item.displaySummary
                      .allowAccountTypes"
                    class="accountBadge me-1"
                    :class="['type1', 'type2', 'type3'][index % 3]"
                    :key="'type_' + index"
                    >{{ $t("type.shortAccount." + acc.accountType) }}</span
                  ></span
                >
                <span v-if="item.serviceType == ReferralServiceType.Agent">
                  <span
                    v-for="(acc, index) in item.displaySummary.schema"
                    class="accountBadge me-1"
                    :class="['type1', 'type2', 'type3'][index % 3]"
                    :key="'type_' + index"
                    >{{ $t("type.shortAccount." + acc.accountType) }}</span
                  ></span
                >
              </div>
              <div>
                {{ $t("type.accountRole." + item.serviceType) }} -
                {{
                  $t(
                    "type.language." +
                      (item.displaySummary?.language == undefined
                        ? getLanguage
                        : item.displaySummary?.language)
                  )
                }}
              </div>
            </div>
            <div class="d-flex justify-content-between align-items-center mt-2">
              <el-switch
                v-model="item.status"
                style="font-size: 18px"
                :active-text="$t('status.active')"
                :inactive-text="$t('fields.inactive')"
                inline-prompt
                :active-value="0"
                :inactive-value="1"
                @change="changeStatus(item)"
              >
              </el-switch>
              <button
                type="button"
                class="btn btn-bordered btn-primary btn-sm fs"
                @click="showDetail(item.code)"
              >
                {{ $t("title.rebateSettings") }}
              </button>

              <CopyReferralLink
                :code="item.code"
                :siteId="salesAccount.siteId"
                :language="
                  item.displaySummary?.language == undefined
                    ? getLanguage
                    : item.displaySummary?.language
                "
              />
            </div>
          </el-card>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import { ReferralServiceType } from "@/core/types/ReferralServiceType";
import { getLanguage } from "@/core/types/LanguageTypes";
import CopyReferralLink from "@/components/CopyReferralLink.vue";

const title = inject<string>("title");
const data = inject<any>("data");
const isLoading = inject<boolean>("isLoading");
const editCode = inject<any>("editCode");
const showDetail = inject<any>("showDetail");
const changeStatus = inject<any>("changeStatus");
const salesAccount = inject<any>("salesAccount");
</script>
<style lang="css" scoped>
.modal-header {
  --bs-modal-header-padding: 0.5rem 0.5rem !important;
}

@media (max-width: 768px) {
  .fs {
    font-size: 0.65rem;
  }
  .btn-outline-primary {
    border: 1px #000 solid !important;
  }
}
</style>
