<template>
  <div
    class="modal fade"
    id="kt_modal_iblibk_detail"
    tabindex="-1"
    aria-hidden="true"
    ref="IBLinkDetailModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-750px">
      <div class="modal-content">
        <div class="modal-header" id="kt_modal_new_address_header">
          <h2>IB Link Detail</h2>

          <div data-bs-dismiss="modal">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <!-- ------------------------------------------------------------------ -->
        <div class="form-outer">
          <table
            class="table table-row-dashed table-row-gray-200 align-middle gs-0 gy-4"
          >
            <tbody
              class="fw-semibold text-gray-600"
              v-for="(accType, index) in accountType"
              :key="index"
            >
              <tr>
                <div class="d-flex align-items-center mt-9 mb-3">
                  <div
                    class="vertical-line"
                    style="
                      border-left: 3px solid #800020;
                      height: 16px;
                      margin-right: 15px;
                    "
                  ></div>
                  <div class="fw-500 fs-4">{{ accType }}</div>
                </div>
              </tr>
              <tr class="text-center" style="border-bottom: 1px solid #000">
                <td class="table-title-gray">Category</td>
                <td class="table-title-gray">Total Rebate</td>
                <td class="table-title-gray">Personal Rebate</td>
                <td class="table-title-gray">Remain Rebate for Sub-IB</td>
                <td class="table-title-gray">Pips</td>
                <td class="table-title-gray">Commission</td>

                <td class="table-title-blue" colspan="2">
                  <div
                    class="d-flex align-items-center justify-content-center p-2"
                    style="cursor: pointer"
                  >
                    <span>%</span>
                  </div>
                </td>
              </tr>
              <tr
                class="text-center"
                v-for="(item, index) in ibLinkDetail.supplement
                  .AllocationSchemas[accType]"
                :key="index"
              >
                <td>
                  {{
                    $t(
                      "type.productCategory." +
                        props.productCategory.find((obj) => obj.id === item.cid)
                          .name
                    )
                  }}
                </td>
                <td>
                  <span v-if="isBroker">{{
                    ibLinkDetail.supplement.BaseSchemas[accType][index].r
                  }}</span>

                  <span v-else>
                    {{ rebateRemainSchema[accType.toLowerCase()][index].r }}
                  </span>
                </td>
                <td>
                  {{ item.r }}
                </td>
                <td>
                  <span v-if="isBroker">{{
                    ibLinkDetail.supplement.BaseSchemas[accType][index].r -
                    item.r
                  }}</span>

                  <span v-else>
                    {{
                      rebateRemainSchema[accType.toLowerCase()][index].r -
                      item.r
                    }}
                  </span>
                </td>
                <td>
                  {{ ibLinkDetail.supplement.BaseSchemas[accType][index].p }}
                </td>
                <td>
                  {{ ibLinkDetail.supplement.BaseSchemas[accType][index].c }}
                </td>
                <td
                  v-if="index == 0"
                  :rowspan="props.productCategory.length"
                  style="border-left: 1px solid #f5f5f5"
                >
                  <el-input class="w-60px me-3" :value="item.cr" disabled />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import IbService from "../services/IbService";
import { AccountTypes } from "@/core/types/AccountInfos";
import { showModal, hideModal } from "@/core/helpers/dom";

const IBLinkDetailModalRef = ref<null | HTMLElement>(null);
const ibLinkDetail = ref();
const isBroker = ref(false);

const rebateRemainSchema = ref({} as any);

const accountType = ref([] as any);

const props = defineProps<{
  productCategory: any;
}>();

const show = async (
  _code: string,
  _isBroker: boolean,
  _rebateRemainSchema: any
) => {
  accountType.value = [];
  isBroker.value = _isBroker;
  rebateRemainSchema.value = _rebateRemainSchema;
  console.log("rebate remain: ", rebateRemainSchema.value);
  showModal(IBLinkDetailModalRef.value);

  try {
    ibLinkDetail.value = await IbService.getIbLinkDetail(_code);

    ibLinkDetail.value.supplement.AccountTypes.forEach((element) => {
      switch (element) {
        case AccountTypes.Standard:
          accountType.value.push("Standard");
          break;
        case AccountTypes.Alpha:
          accountType.value.push("Alpha");
          break;
        case AccountTypes.Advantage:
          accountType.value.push("Advantage");
          break;
      }
    });
  } catch (error) {
    // console.log(error);
  }
};

const hide = () => {
  hideModal(IBLinkDetailModalRef.value);
};

defineExpose({
  hide,
  show,
});
</script>
