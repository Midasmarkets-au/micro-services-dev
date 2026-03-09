<template>
  <div class="table-responsive">
    <table
      class="table table-row-dashed table-row-gray-200 align-middle gs-0 gy-4"
    >
      <tbody class="fw-semibold text-gray-600">
        <tr class="text-center" style="border-bottom: 1px solid #000">
          <td class="table-title-gray">Category</td>
          <td class="table-title-gray">Total Rebate</td>
          <td class="table-title-gray">Personal Rebate</td>
          <td class="table-title-gray">Remain Rebate for Sub-IB</td>

          <td v-if="isBroker" class="table-title-blue" colspan="2">
            <div
              class="d-flex align-items-center justify-content-center p-2"
              style="cursor: pointer"
              @click="(pcDropdown = !pcDropdown), (optionDropdown = false)"
            >
              <span>{{ selectedPC }}</span>
              <i class="fa-solid fa-angle-down ms-3" style="color: #ffffff"></i>
            </div>

            <!-- dropdown -->
            <div v-if="pcDropdown" style="position: relative">
              <div class="rebateDropdown">
                <div class="dropdownItem" @click="selectPCFn('Pips')">Pips</div>
                <div class="dropdownItem" @click="selectPCFn('Commission')">
                  Commission
                </div>
              </div>
            </div>

            <div
              class="d-flex align-items-center justify-content-center p-2"
              style="cursor: pointer"
              @click="(optionDropdown = !optionDropdown), (pcDropdown = false)"
            >
              <span>Options</span>
              <i class="fa-solid fa-angle-down ms-3" style="color: #ffffff"></i>
            </div>

            <!-- dropdown -->
            <div v-if="optionDropdown" style="position: relative">
              <div v-if="selectedPC == 'Pips'" class="rebateDropdown">
                <div
                  v-for="(item, index) in props.rebateSchemaOption.pipOptions"
                  class="dropdownItem"
                  @click="selectValFn(item)"
                  :key="index"
                >
                  {{ item }}
                </div>
              </div>
              <div
                v-else-if="selectedPC == 'Commission'"
                class="rebateDropdown"
              >
                <div
                  v-for="(item, index) in props.rebateSchemaOption
                    .commissionOptions"
                  class="dropdownItem"
                  :key="index"
                  @click="selectValFn(item)"
                >
                  $ {{ item }}
                </div>
              </div>
            </div>
          </td>
          <td v-else class="table-title-gray">Percentage</td>
        </tr>
        <tr class="text-center" v-for="(item, index) in formTable" :key="index">
          <td>{{ $t("type.productCategory." + item.name) }}</td>
          <td>{{ item.total }}</td>
          <td>
            <el-input class="w-60px me-3" type="string" v-model="item.r" />
          </td>
          <td>{{ item.total - item.r }}</td>
          <td v-if="isBroker" class="w-150px pe-3">
            <span v-if="selectedPC == 'Pips'">{{ pcValue }} pips</span>
            <span v-if="selectedPC == 'Commission'">$ {{ pcValue }}</span>
          </td>
          <td
            v-if="index == 0"
            :rowspan="props.productCategory.length"
            style="border-left: 1px solid #f5f5f5"
          >
            <div
              class="d-flex align-items-center justify-content-center p-2"
              style="cursor: pointer"
            >
              <span
                ><el-input
                  class="w-60px me-3"
                  type="string"
                  v-model="percentage"
                />
                %</span
              >
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script lang="ts" setup>
import { ref } from "vue";

const percentage = ref("0");
const pcDropdown = ref(false);
const optionDropdown = ref(false);
const formTable = ref([] as any);
const selectedPC = ref("Pips");
const pcValue = ref(0);

const props = defineProps<{
  productCategory: any;
  rebateSchemaOption: any;
  isBroker: boolean;
  rebateRemainSchema: any;
}>();

const emits = defineEmits<{
  (
    e: "prepareData",
    _form: string,
    _baseSchema: any,
    _allocateSchema: any
  ): void;
}>();

const selectPCFn = (val: string) => {
  pcValue.value = 0;
  selectedPC.value = val;
  pcDropdown.value = false;
};

const selectValFn = (val: number) => {
  pcValue.value = val;
  optionDropdown.value = false;
};

const generateForm = () => {
  formTable.value = [];
  props.productCategory.forEach((element, index) => {
    formTable.value.push({
      cid: element.key,
      name: element.value,
      total: props.isBroker
        ? props.rebateSchemaOption.rate
        : props.rebateRemainSchema[index].r,
      r: 0,
      c: 0,
      p: 0,
    });
  });

  console.log(formTable.value);
};

const collectDate = () => {
  // console.log("Collect stdFormData");

  var baseSchema = [] as any;
  var allocateSchema = [] as any;
  var _p = 0;
  var _c = 0;

  if (selectedPC.value == "Pips") {
    _p = pcValue.value;
  } else {
    _c = pcValue.value;
  }

  formTable.value.forEach((element) => {
    baseSchema.push({
      cid: element.cid,
      r: props.rebateSchemaOption.rate,
      c: _c,
      p: _p,
    });

    allocateSchema.push({
      cid: element.cid,
      r: parseInt(element.r),
      cr: parseInt(percentage.value),
    });
  });

  emits("prepareData", "standard", baseSchema, allocateSchema);
};

defineExpose({
  generateForm,
  collectDate,
});
</script>

<style>
.table-title-gray {
  background-color: #f5f7fa !important;
}

.table-title-blue {
  color: white !important;
  background-color: #0053ad !important;
}

.rebateDropdown {
  position: absolute;
  overflow: hidden;
  right: 10px;

  color: black;
  background-color: white;
  width: 172px;
  filter: drop-shadow(0px 4px 14px rgba(0, 0, 0, 0.1));
  border-radius: 8px;
}

.dropdownItem {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 40px;
  cursor: pointer;
  border: 1px solid #f5f7fa;
}

.dropdownItem:hover {
  background-color: #f5f7fa;
}
</style>
