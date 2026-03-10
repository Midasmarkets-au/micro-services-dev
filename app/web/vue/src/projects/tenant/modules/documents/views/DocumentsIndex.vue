<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-button type="success" @click="showCreate" plain>{{
          $t("action.create")
        }}</el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant">
        <thead>
          <tr>
            <th>{{ $t("fields.documentId") }}</th>
            <th>{{ $t("fields.name") }}</th>
            <th>{{ $t("fields.uploadedLanguages") }}</th>
            <th>{{ $t("action.update") }}</th>
            <th>{{ $t("action.action") }}</th>
            <!-- <th>Last Operator</th> -->
          </tr>
        </thead>
        <tbody v-if="isLoading" style="height: 300px">
          <tr>
            <td colspan="12">
              <scale-loader :color="'#ffc730'"></scale-loader>
            </td>
          </tr>
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="item in data" :key="item.id">
            <td>{{ item.id }}</td>
            <td>{{ item.title }}</td>
            <td>
              <el-tag
                v-for="(lang, key) in item.languages"
                :key="key"
                plain
                class="ms-3"
                type="warning"
              >
                {{ key }}
              </el-tag>
            </td>
            <td>
              <el-button
                type="primary"
                @click="upload(item)"
                :loading="isLoading"
                plain
              >
                {{ $t("action.upload") }}
              </el-button>
              <el-button
                type="success"
                @click="UploadPdfFile(item)"
                v-if="$can('SuperAdmin')"
                plain
              >
                {{ $t("action.upload") }} PDF (SuperAdmin)
              </el-button>
            </td>
            <td>
              <el-button
                type="warning"
                @click="viewHistoricalData(item)"
                :loading="isLoading"
                plain
              >
                {{ $t("action.view") }}
              </el-button>
            </td>
            <!-- <td>
              <el-popover trigger="hover" placement="left" :width="400">
                <template #default>
                  <table class="table table-tenant">
                    <thead>
                      <tr>
                        <th>Language</th>
                        <th>Name</th>
                        <th>Updated At</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr
                        v-for="(operator, index) in item.operator_info"
                        :key="index"
                      >
                        <td>{{ index }}</td>
                        <td>{{ operator.name }}</td>
                        <td>
                          <TimeShow
                            type="GMToneLiner"
                            :date-iso-string="operator.updatedAt"
                          />
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </template>
                <template #reference>
                  <el-icon><Memo /></el-icon>
                </template>
              </el-popover>
            </td> -->
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <UploadDocuments ref="uploadDocsRef" @submitted="fetchData(1)" />
  <UploadPdf ref="uploadPdfRef" />
  <HistoricalDocuments ref="historicalDocsRef" />
  <AddDocuments ref="addDocsRef" @submitted="fetchData(1)" />
</template>
<script lang="ts" setup>
import { ref, onMounted } from "vue";
import DocsServices from "../services/DocsServices";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UploadDocuments from "../components/DocumentsIndex/UploadDocuments.vue";
import UploadPdf from "../components/DocumentsIndex/UploadPdf.vue";
import HistoricalDocuments from "../components/DocumentsIndex/HistoricalDocuments.vue";
import AddDocuments from "../components/DocumentsIndex/AddDocuments.vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { useStore } from "@/store";
const store = useStore();
const user = store.state.AuthModule.user;
const site = ref(user.tenancy == "au" ? "ba" : user.tenancy);
const isLoading = ref(false);
const data = ref<any>([]);
const uploadDocsRef = ref<any>(null);
const uploadPdfRef = ref<any>(null);
const historicalDocsRef = ref<any>(null);
const addDocsRef = ref<any>(null);
const docsLanguages = ref<any>({});

const criteria = ref<any>({
  page: 1,
  size: 20,
  site: site.value,
});

const upload = (item: any) => {
  uploadDocsRef.value.show(item, docsLanguages.value);
};

const UploadPdfFile = (item: any) => {
  uploadPdfRef.value.show(item, docsLanguages.value);
};

const viewHistoricalData = (item: any) => {
  historicalDocsRef.value.show(item);
};

const showCreate = () => {
  addDocsRef.value.show();
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  try {
    criteria.value.page = _page;
    const res = await DocsServices.queryDocumentsList(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
    docsLanguages.value = res.languages;
  } catch (error) {
    console.log(error);
    MsgPrompt.error("Fetch data failed");
  }
  isLoading.value = false;
};

onMounted(async () => {
  await fetchData(1);
});
</script>
