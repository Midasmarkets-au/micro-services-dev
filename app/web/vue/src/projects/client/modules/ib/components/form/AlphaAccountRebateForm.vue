<template>
  <div class="table-responsive">
    <table
      class="table table-row-dashed table-row-gray-200 align-middle gs-0 gy-4"
    >
      <thead>
        <tr class="border-0">
          <th class="p-0"></th>
          <th class="p-0 min-w-150px"></th>
          <th class="p-0 min-w-120px"></th>
          <th class="p-0 min-w-110px"></th>
        </tr>
      </thead>

      <tbody class="fw-semibold text-gray-600">
        <tr class="text-center" style="border-bottom: 1px solid #000">
          <td class="table-title-gray">Category</td>
          <td class="table-title-gray">Total Rebate</td>
          <td class="table-title-gray">Personal Rebate</td>
          <td class="table-title-gray">Remain Rebate for Sub-IB</td>
        </tr>
        <tr
          class="text-center"
          v-for="(item, index) in allocationTable"
          :key="index"
        >
          <td>{{ $t("type.productCategory." + item.name) }}</td>
          <td>{{ item.total }}</td>
          <td>
            <el-input class="w-60px me-3" type="string" v-model="item.r" />
          </td>
          <td>{{ item.total - item.r }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script lang="ts" setup>
import { ref } from "vue";

const allocationTable = ref([] as any);

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

const generateForm = () => {
  allocationTable.value = [];
  props.productCategory.forEach((element, index) => {
    allocationTable.value.push({
      cid: element.id,
      name: element.name,
      total: props.isBroker
        ? props.rebateSchemaOption.rate
        : props.rebateRemainSchema[index].r,
      r: 0,
    });
  });
};

const collectDate = () => {
  // console.log("Collect stdFormData");

  var baseSchema = [] as any;
  var allocateSchema = [] as any;

  allocationTable.value.forEach((element) => {
    baseSchema.push({
      cid: element.cid,
      r: props.rebateSchemaOption.rate,
      c: 0,
      p: 0,
    });
    allocateSchema.push({
      cid: element.cid,
      r: parseInt(element.r),
      cr: 0,
    });
  });
  emits("prepareData", "alpha", baseSchema, allocateSchema);
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
</style>
