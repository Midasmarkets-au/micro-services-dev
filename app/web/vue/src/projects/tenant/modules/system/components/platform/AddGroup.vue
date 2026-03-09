<template>
  <SimpleForm
    ref="modalRef"
    :title="title"
    :is-loading="isLoading"
    :submit="submit"
    :before-close="hide"
    :width="950"
    :overflow="true"
  >
    <div class="d-flex justify-content-between h-800px">
      <div class="card overflow-auto">
        <div class="card-header"><div class="card-title">Source</div></div>
        <div class="card-body">
          <el-table :data="sourceData" class="w-350px">
            <el-table-column
              prop="name"
              label="Group"
              width="250"
            ></el-table-column>

            <el-table-column>
              <template #default="scope">
                <el-button
                  type="success"
                  :disabled="!scope.row.toggle"
                  @click="add(scope.row)"
                >
                  Add
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>
      <div class="d-flex align-items-center justify-content-center">
        <el-button size="large" :icon="ArrowRight" />
      </div>
      <div class="card overflow-auto">
        <div class="card-header"><div class="card-title">Target</div></div>
        <div class="card-body">
          <div class="d-flex gap-20">
            <el-table :data="targetData" class="w-350px">
              <el-table-column label="Group" width="220"
                ><template #default="scope">
                  <span
                    :class="[
                      scope.row.source ? 'text-success' : '',
                      scope.row.toggle ? '' : 'text-decoration-line-through',
                    ]"
                    >{{ scope.row.name }}</span
                  >
                </template>
              </el-table-column>
              <el-table-column>
                <template #default="scope">
                  <el-button
                    v-if="scope.row.toggle"
                    type="danger"
                    @click="remove(scope.row)"
                  >
                    Remove
                  </el-button>
                  <el-button v-else type="warning" @click="restore(scope.row)">
                    Restore
                  </el-button>
                </template>
              </el-table-column>
            </el-table>
          </div>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import SystemService from "../services/SystemService";
import { ArrowRight } from "@element-plus/icons-vue";
import MsgPrompt from "@/components/MsgPrompt.vue";
const isLoading = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const targetData = ref(<any>[]);
const sourceData = ref(<any>[]);
const title = ref("");

const emits = defineEmits<{
  (e: "submit"): void;
}>();
const submit = async () => {
  isLoading.value = true;
  try {
    emits("submit");
    // const res = await SystemServices.updateGroups({
    //   description: title.value,
    //   groups: targetData.value
    //     .filter((i: any) => i.toggle)
    //     .map((i: any) => i.name),
    // });
    // if (res) {
    //   emits("submit");
    //   hide();
    // }
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};
const show = (item: any) => {
  title.value = item.description;
  targetData.value = [];
  sourceData.value = [];
  item.groups.forEach((group: any) => {
    targetData.value.push({
      name: group,
      toggle: true,
    });
  });
  item.groups.forEach((group: any) => {
    sourceData.value.push({
      name: group + " Source",
      toggle: true,
      source: true,
    });
  });
  modalRef.value?.show();
};
const remove = (_item: any) => {
  const item = targetData.value.find((i: any) => i.name === _item.name);
  if (item) {
    item.toggle = false;
  }
};
const add = (item: any) => {
  const _item = sourceData.value.find((i: any) => i.name === item.name);
  if (_item) {
    _item.toggle = false;
  }
  targetData.value.unshift({
    name: item.name,
    toggle: true,
    source: item.source,
  });
};

const restore = (_item: any) => {
  const item = targetData.value.find((i: any) => i.name === _item.name);
  if (item) {
    item.toggle = true;
  }
};

const hide = () => {
  modalRef.value?.hide();
};

defineExpose({
  show,
  hide,
});
</script>
<style scoped lang="scss">
.border-bottom {
  border-bottom: 1px solid #acb1bc;
}
</style>
