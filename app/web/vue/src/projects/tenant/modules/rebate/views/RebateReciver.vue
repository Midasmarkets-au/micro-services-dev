<template>
  <div>
    <button @click="addRow">Add Row</button>
    <table>
      <thead>
        <tr>
          <th>Name</th>
          <th>Age</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <transition-group name="row" mode="out-in">
          <tr
            v-for="(row, index) in rows"
            :key="index"
            :class="{
              removing: row.removing,
              'moving-up': row.movingUp,
              adding: row.adding,
            }"
          >
            <td>{{ row.name }}</td>
            <td>{{ row.age }}</td>
            <td>
              <button @click="removeRow(row)">Remove</button>
            </td>
          </tr>
        </transition-group>
      </tbody>
    </table>
  </div>
</template>

<script>
export default {
  data() {
    return {
      rows: [
        {
          name: "Alice",
          age: 25,
          removing: false,
          movingUp: false,
          adding: false,
        },
        {
          name: "Bob",
          age: 30,
          removing: false,
          movingUp: false,
          adding: false,
        },
        {
          name: "Charlie",
          age: 35,
          removing: false,
          movingUp: false,
          adding: false,
        },
      ],
    };
  },
  methods: {
    addRow() {
      const newRow = {
        name: "New Row",
        age: 20,
        removing: false,
        movingUp: false,
        adding: true,
      };
      this.rows.unshift(newRow);
      setTimeout(() => {
        newRow.adding = false;
      }, 500); // adjust delay as needed
    },
    removeRow(row) {
      row.removing = true;
      const index = this.rows.indexOf(row);
      for (let i = index + 1; i < this.rows.length; i++) {
        this.rows[i].movingUp = true;
      }
      setTimeout(() => {
        this.rows.splice(index, 1);
        for (let i = index; i < this.rows.length; i++) {
          this.rows[i].movingUp = false;
        }
      }, 500); // adjust delay as needed
    },
  },
};
</script>

<style>
.row {
  transition: transform 0.5s ease;
}

.row-enter-active,
.row-leave-active {
  transition: transform 0.5s ease;
}

.row-enter,
.row-leave-to {
  opacity: 0;
  transform: translateY(-30px);
}

.row-move {
  transition: transform 0.5s ease;
}

.removing {
  opacity: 0;
  transform: translateY(-30px);
  height: 0;
  margin: 0;
  padding: 0;
  border: none;
}

.moving-up {
  transition: transform 0.5s ease;
  transform: translateY(-30px);
}

.adding {
  transition: transform 0.5s ease;
  transform: translateY(60px);
}
</style>
