<template>
  <div v-if="isMobile">
    <ManageLinkMobile />
    <TableFooter @page-change="fetchData" :criteria="{ status: 0 }" />
  </div>
  <div v-else>
    <div class="font-medium fs-2 card-title">{{ $t("action.manageLink") }}</div>
    <div class="overflow-auto" style="white-space: nowrap">
      <table class="table align-middle table-row-bordered gy-5">
        <thead>
          <tr class="gs-0">
            <th>{{ $t("fields.linkName") }}</th>
            <th class="text-center">{{ $t("fields.referCode") }}</th>
            <th class="text-center">{{ $t("fields.linkType") }}</th>
            <th class="text-center">{{ $t("fields.language") }}</th>
            <!-- <th class="text-center">{{ $t("tip.numberOfIB") }}</th> -->
            <th class="text-center" v-if="projectConfig?.rebateEnabled">
              {{ $t("title.rebateSettings") }}
            </th>
            <th class="text-center">
              {{ $t("title.autoCreateAccount") }}
            </th>
            <th class="text-center">{{ $t("fields.actions") }}</th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>

        <tbody v-else-if="!isLoading && ibLinks.data.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else>
          <tr v-for="(item, index) in ibLinks.data" :key="index">
            <td>
              {{ item.name
              }}<i
                class="fa-solid fa-pen-to-square ms-2"
                style="cursor: pointer"
                @click="editCode(item)"
              ></i>
            </td>
            <td class="text-center">{{ item.code }}</td>
            <td class="text-center">
              {{ $t("type.accountRole." + item.serviceType) }}
            </td>
            <td class="text-center">
              {{
                $t(
                  "type.language." +
                    (item.displaySummary?.language == undefined
                      ? getLanguage
                      : item.displaySummary?.language)
                )
              }}
            </td>
            <!-- <td class="text-center">{{ item.referredCount }}</td> -->
            <td class="text-center" v-if="projectConfig?.rebateEnabled">
              <i
                class="fa-regular fa-eye"
                style="cursor: pointer"
                @click="showDetail(item.code)"
              ></i>
            </td>
            <td class="text-center">
              {{
                item.displaySummary?.isAutoCreatePaymentMethod === 1
                  ? $t("action.yes")
                  : $t("action.no")
              }}
            </td>
            <td class="text-center">
              <CopyReferralLink
                :code="item.code"
                :siteId="salesAccount.siteId"
                :language="
                  item.displaySummary?.language == undefined
                    ? getLanguage
                    : item.displaySummary?.language
                "
              />
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="{ status: 0 }" />
    </div>
  </div>

  <SalesLinkDetailModal ref="SalesLinkDetailRef" />
  <EditReferralLink ref="EditReferralLinkRef" @fetchData="fetchData" />
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { ref, onMounted, computed, provide } from "vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { getLanguage } from "@/core/types/LanguageTypes";
import { isMobile } from "@/core/config/WindowConfig";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SalesService from "../services/SalesService";
import SalesLinkDetailModal from "./SalesLinkDetail.vue";
import CopyReferralLink from "@/components/CopyReferralLink.vue";
import EditReferralLink from "@/projects/client/modules/sales/components/modal/EditReferralLink.vue";
import ManageLinkMobile from "./link/ManageLinkMobile.vue";

const { t } = useI18n();
const store = useStore();
const isLoading = ref(true);
const ibLinks = ref({} as any);
const projectConfig: PublicSetting = store.state.AuthModule.config;
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
const SalesLinkDetailRef = ref<InstanceType<typeof SalesLinkDetailModal>>();
const EditReferralLinkRef = ref<InstanceType<typeof EditReferralLink>>();

const fetchData = async () => {
  isLoading.value = true;
  try {
    ibLinks.value = await SalesService.getIbLinks({ status: 0 });
    ibLinks.value.data = ibLinks.value.data.filter((item) =>
      item.code.startsWith("RS")
    );
  } catch (error) {
    MsgPrompt.error(t("error.oopsErrorAccured"), "5128");
  } finally {
    isLoading.value = false;
  }
};

const showDetail = (_code: string) => {
  SalesLinkDetailRef.value?.show(_code);
};

const editCode = (item: any) => {
  EditReferralLinkRef.value?.show(item);
};

provide("isLoading", isLoading);
provide("ibLinks", ibLinks);
provide("editCode", editCode);
provide("showDetail", showDetail);
provide("salesAccount", salesAccount);

onMounted(() => {
  fetchData();
});

defineExpose({
  fetchData,
});
</script>
