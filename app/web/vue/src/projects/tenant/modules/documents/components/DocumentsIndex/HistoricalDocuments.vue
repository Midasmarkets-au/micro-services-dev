<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('fields.viewDocumentsRecords') + ': ' + title"
    width="1000"
    align-center
    :before-close="hide"
  >
    <div class="card">
      <div class="card-header">
        <div class="card-title">
          <el-select
            v-model="criteria.language"
            :placeholder="$t('fields.selectLanguage')"
            :disabled="isLoading"
            @change="fetchData(1)"
            clearable
          >
            <el-option
              v-for="(lang, key) in languages"
              :key="key"
              :label="lang"
              :value="key"
            />
          </el-select>
        </div>
      </div>
      <div class="card-body">
        <table class="table table-tenant">
          <thead>
            <tr>
              <th>{{ $t("fields.version") }}</th>
              <th>{{ $t("fields.language") }}</th>
              <th>{{ $t("fields.uploadedAt") }}</th>
              <th>{{ $t("fields.uploadedBy") }}</th>
              <th>{{ $t("action.action") }}</th>
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
              <td>{{ item.reference }}</td>
              <td>{{ item.language }}</td>
              <td>
                <TimeShow
                  type="GMToneLiner"
                  :date-iso-string="item.updated_at"
                />
              </td>
              <td>
                {{ item.operator_info.name }} - {{ item.operator_info.email }}
              </td>
              <td>
                <el-button
                  type="primary"
                  :loading="isLoading"
                  tag="a"
                  :href="prefixLink + item.link"
                  target="_blank"
                  rel="noopener noreferrer"
                  plain
                >
                  {{ $t("action.download") }}
                </el-button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.close")
        }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import { prefixLink } from "@/core/data/bcrDocs";
import DocsServices from "../../services/DocsServices";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
const dialogRef = ref(false);
const isLoading = ref(false);
const title = ref("");
const languages = ref<any>({});
const data = ref<any>([]);
const id = ref(0);
const criteria = ref<any>({
  page: 1,
  size: 15,
});

const site = ref("bvi");
const show = async (item) => {
  dialogRef.value = true;
  title.value = item.title;
  site.value = item.site;
  id.value = item.id;
  languages.value = item.languages;
  await fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  try {
    criteria.value.page = _page;
    const response = await DocsServices.queryDocumentHistoryById(
      id.value,
      criteria.value
    );
    data.value = response.data;
    criteria.value = response.criteria;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const hide = () => {
  dialogRef.value = false;
  data.value = [];
  criteria.value = { page: 1, size: 15 };
};

defineExpose({ show });
</script>
