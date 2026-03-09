<template>
  <el-popover trigger="click" placement="left" :width="800" @show="onShow">
    <template #default>
      <div class="table-container" v-if="isShow">
        <table class="table table-row-dashed table-hover table-layout-fixed">
          <thead style="background-color: aliceblue; position: sticky; top: 0">
            <tr class="text-muted fw-bold text-uppercase">
              <th class="text-center">{{ $t("fields.name") }}</th>
              <th>{{ $t("fields.currency") }}</th>
              <th>{{ $t("fields.volume") }}</th>
              <th>{{ $t("fields.amount") }}</th>
            </tr>
            <tr class="text-success fw-bold text-uppercase">
              <th class="text-center">{{ $t("title.total") }}</th>
              <th>{{ $t(`type.currency.${totalData.currency}`) }}</th>
              <th>{{ totalData.volume }}</th>
              <th>
                <BalanceShow
                  :currency-id="Number(totalData.currency)"
                  :balance="totalData.amounts"
                  :class="totalData.amounts > 0 ? '' : 'text-danger'"
                />
              </th>
            </tr>
          </thead>
          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && data.length === 0">
            <NoDataBox />
          </tbody>
          <tbody v-else>
            <tr v-for="(item, key) in data" :key="key">
              <td class="text-center">{{ key }}</td>
              <td
                v-for="(amountValue, amountKey) in item.amounts"
                :key="amountKey"
              >
                <span class="me-1">{{ $t(`type.currency.${amountKey}`) }}</span>
              </td>
              <td>{{ item.volume }}</td>

              <td
                v-for="(amountValue, amountKey) in item.amounts"
                :key="amountKey"
              >
                <BalanceShow
                  :currency-id="Number(amountKey)"
                  :balance="amountValue"
                  :class="amountValue > 0 ? '' : 'text-danger'"
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
    <template #reference>
      <el-button :icon="Memo" size="small" circle></el-button>
    </template>
  </el-popover>
</template>
<script setup lang="ts">
import { ref } from "vue";
import AccountService from "../../services/AccountService";
import { Memo } from "@element-plus/icons-vue";

const props = defineProps<{
  item: any;
  from: any;
  to: any;
}>();

const isLoading = ref(false);
const isShow = ref(false);
const data = ref(<any>[]);
const totalData = ref(<any>[]);

const fecthData = async () => {
  isLoading.value = true;
  try {
    data.value = await AccountService.getChildRebateStat({
      uid: props.item.uid,
      from: props.from,
      to: props.to,
    });
    var currency = data.value[Object.keys(data.value)[0]].amounts;
    currency = Object.keys(currency)[0];
    const total = {
      volume: Object.values(data.value).reduce(
        (acc: number, cur: any) => acc + cur.volume,
        0
      ),
      amounts: Object.values(data.value).reduce(
        (acc: any, cur: any) => acc + cur.amounts[currency],
        0
      ),
      currency: currency,
    };
    totalData.value = total;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const onShow = () => {
  isShow.value = true;
  fecthData();
};
</script>

<style scoped>
.table-container {
  max-height: 500px;
  overflow-x: auto;
}
</style>
