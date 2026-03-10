<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-header">
      <div class="card-title">
        <el-form
          class="d-flex gap-5"
          :model="formData"
          :rules="formRule"
          ref="formRef"
        >
          <el-form-item>
            <el-select v-model="selectedType">
              <el-option
                v-for="item in typeOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item prop="key"
            ><el-input
              v-model="formData.key"
              class="w-250px"
              placeholder="Group / Sales code / login number"
              clearable
            ></el-input
          ></el-form-item>
          <el-form-item prop="startTime">
            <el-date-picker
              v-model="formData.startTime"
              type="datetime"
              placeholder="Start Time"
              format="YYYY-MM-DD HH:mm:ss"
              date-format="MMM DD, YYYY"
              time-format="HH:mm"
            />
          </el-form-item>
          <el-form-item prop="endTime">
            <el-date-picker
              v-model="formData.endTime"
              type="datetime"
              placeholder="End Time"
              format="YYYY-MM-DD HH:mm:ss"
              date-format="MMM DD, YYYY"
              time-format="HH:mm"
            />
          </el-form-item>
          <div>
            <el-button type="primary" @click="fetchData(formRef)">{{
              $t("action.search")
            }}</el-button>
            <el-button type="info" plain @click="clear(formRef)">{{
              $t("action.reset")
            }}</el-button>
            <el-button type="success" @click="copy()">{{
              $t("action.copy")
            }}</el-button>
          </div>
        </el-form>
      </div>
    </div>
    <div class="card-body">
      <div v-if="isLoading">
        <LoadingRing />
      </div>
      <div v-else-if="!isLoading && detail.length === 0">
        <NoDataBox />
      </div>
      <div v-else></div>
      <div>
        <p>{{ detail }}</p>
        <!-- Open -->
        <div class="row rol-cols-2">
          <div class="card p-4 col mx-4">
            <p class="fs-1 text-uppercase fw-bold">
              {{ $t("fields.openLong") }}
            </p>
            <div class="row row-cols-2">
              <div class="col">
                <p class="fs-2 fw-bold text-success">AUDUSD</p>
                <div class="d-flex gap-4 text-center">
                  <div class="border-end pe-4">
                    <p class="fs-4 text-black-50 fw-bold">Avg.</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">
                      123.45678
                    </p>
                  </div>
                  <div class="border-end pe-4">
                    <p class="fs-4 text-black-50 fw-bold">Between</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">
                      04:00 - 05:00
                    </p>
                  </div>
                  <div>
                    <p class="fs-4 text-black-50 fw-bold">Lot</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">1.00</p>
                  </div>
                </div>
              </div>
              <div class="col">
                <p class="fs-2 fw-bold text-success">AUDUSD</p>
                <div class="d-flex gap-4 text-center">
                  <div class="border-end pe-4">
                    <p class="fs-4 text-black-50 fw-bold">Avg.</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">
                      123.45678
                    </p>
                  </div>
                  <div class="border-end pe-4">
                    <p class="fs-4 text-black-50 fw-bold">Between</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">
                      04:00 - 05:00
                    </p>
                  </div>
                  <div>
                    <p class="fs-4 text-black-50 fw-bold">Lot</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">1.00</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="card p-4 col mx-4">
            <p class="fs-1 text-uppercase fw-bold">open long</p>
            <div class="row row-cols-2">
              <div class="col">
                <p class="fs-2 fw-bold text-success">AUDUSD</p>
                <div class="d-flex gap-4 text-center">
                  <div class="border-end pe-4">
                    <p class="fs-4 text-black-50 fw-bold">Avg.</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">
                      123.45678
                    </p>
                  </div>
                  <div class="border-end pe-4">
                    <p class="fs-4 text-black-50 fw-bold">Between</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">
                      04:00 - 05:00
                    </p>
                  </div>
                  <div>
                    <p class="fs-4 text-black-50 fw-bold">Lot</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">1.00</p>
                  </div>
                </div>
              </div>
              <div class="col">
                <p class="fs-2 fw-bold text-success">AUDUSD</p>
                <div class="d-flex gap-4 text-center">
                  <div class="border-end pe-4">
                    <p class="fs-4 text-black-50 fw-bold">Avg.</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">
                      123.45678
                    </p>
                  </div>
                  <div class="border-end pe-4">
                    <p class="fs-4 text-black-50 fw-bold">Between</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">
                      04:00 - 05:00
                    </p>
                  </div>
                  <div>
                    <p class="fs-4 text-black-50 fw-bold">Lot</p>
                    <p class="fs-3 text-secondary-emphasis fw-bold">1.00</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Summary -->
        <div class="card mt-5 p-5">
          <p class="fs-1 text-uppercase fw-bold">summary</p>
          <div class="row">
            <div class="col-6">
              <p>
                <span class="fs-2 text-black-50 fw-bold pe-4">Equity</span>
                <span class="fs-2 text-success fw-bold">12312312312</span>
              </p>
              <p>
                <span class="fs-2 text-black-50 fw-bold pe-4">Timestamp</span>
                <span class="fs-2 text-success fw-bold">04-23-2023</span>
              </p>
            </div>
            <div class="col-6">
              <p class="fs-2 text-black-50 fw-bold pe-4">Price</p>
              <table
                class="align-middle table-row-dashed fs-6 gy-5 w-100 text-center"
              >
                <thead>
                  <tr class="fs-2 fw-bold text-black-50">
                    <th>Symbol</th>
                    <th>Ask</th>
                    <th>Bid</th>
                  </tr>
                </thead>
                <tbody>
                  <tr class="fs-3 fw-bold">
                    <td>
                      <span>AUDUSD</span>
                    </td>
                    <td>
                      <span>123.45678</span>
                    </td>
                    <td>
                      <span>123.45678</span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import { FormInstance } from "element-plus";
import ToolServices from "@/projects/tenant/modules/tools/services/ToolServices";
import Clipboard from "clipboard";

const isLoading = ref(false);
const detail = ref<any>([]);
const submit = ref<any>([]);

const selectedType = ref("group");
const typeOptions = ref([
  { label: "Group", value: "group" },
  { label: "Sales code", value: "salesCode" },
  { label: "Login number", value: "loginNumber" },
]);

const formRef = ref<FormInstance>();
const formData = ref<any>({
  key: "",
  startTime: "",
  endTime: "",
});

const formRule = reactive({
  key: [{ required: true, message: "This field is required", trigger: "blur" }],
  startTime: [
    { required: true, message: "Please select a time", trigger: "blur" },
  ],
  endTime: [
    { required: true, message: "Please select a time", trigger: "blur" },
  ],
});

const fetchData = async (formEl: FormInstance | undefined) => {
  isLoading.value = true;
  try {
    await formEl?.validate((valid) => {
      if (valid) {
        submit.value = {
          startTime: formData.value.startTime,
          endTime: formData.value.endTime,
        };
        submit.value[selectedType.value] = formData.value.key;
        // const res = await ToolServices.getBriefDetail();
        // detail.value = res.data;
      } else {
        return false;
      }
    });
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const copy = () => {
  Clipboard.copy("aaa" as string);
};

const clear = (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  formEl.resetFields();
  submit.value = {};
};
</script>

<style scoped>
.el-form-item {
  margin-top: auto;
  margin-bottom: auto;
}
</style>
